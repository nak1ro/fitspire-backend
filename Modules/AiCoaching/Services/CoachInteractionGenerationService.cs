using System.Text.Json;
using backend.Data;
using backend.Modules.AiCoaching.Configuration;
using backend.Modules.AiCoaching.Contracts;
using backend.Modules.AiCoaching.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace backend.Modules.AiCoaching.Services;

public interface ICoachInteractionGenerationService
{
    Task<bool> ProcessOneAsync(CancellationToken cancellationToken);
}

public sealed class CoachInteractionGenerationService : ICoachInteractionGenerationService
{
    private const int MaximumTransientAttempts = 2;
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);
    private readonly FitspireDbContext _context;
    private readonly ICoachContextSnapshotBuilder _snapshotBuilder;
    private readonly ICoachInteractionOutputValidator _outputValidator;
    private readonly IGenerativeAiClient _aiClient;
    private readonly OpenAiOptions _openAiOptions;
    private readonly AiCoachInteractionOptions _options;
    private readonly ILogger<CoachInteractionGenerationService> _logger;

    public CoachInteractionGenerationService(FitspireDbContext context, ICoachContextSnapshotBuilder snapshotBuilder,
        ICoachInteractionOutputValidator outputValidator, IGenerativeAiClient aiClient, IOptions<OpenAiOptions> openAiOptions,
        IOptions<AiCoachInteractionOptions> options, ILogger<CoachInteractionGenerationService> logger)
    {
        _context = context;
        _snapshotBuilder = snapshotBuilder;
        _outputValidator = outputValidator;
        _aiClient = aiClient;
        _openAiOptions = openAiOptions.Value;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<bool> ProcessOneAsync(CancellationToken cancellationToken)
    {
        if (!_openAiOptions.Enabled)
            return false;

        await RequeueExpiredClaimsAsync(cancellationToken);
        var next = await FindNextPendingWorkAsync(cancellationToken);
        return next switch
        {
            CoachGenerationWork.Message message => await ProcessMessageAsync(message.Id, cancellationToken),
            CoachGenerationWork.DailyBriefing briefing => await ProcessDailyBriefingAsync(briefing.Id, cancellationToken),
            _ => false
        };
    }

    private async Task<bool> ProcessMessageAsync(Guid messageId, CancellationToken cancellationToken)
    {
        var lease = await ClaimMessageAsync(messageId, cancellationToken);
        if (lease is null)
            return false;

        try
        {
            var source = await BuildMessageSourceAsync(lease, cancellationToken);
            if (source is null)
                return true;
            var result = await GenerateAsync(CoachInteractionPromptCatalogue.ConversationInstructions, source.Snapshot.SnapshotJson,
                CoachInteractionStructuredOutputSchema.ConversationName, CoachInteractionStructuredOutputSchema.CreateConversation(),
                lease.UserId, CoachInteractionPromptCatalogue.ConversationVersion, cancellationToken);
            var answerJson = _outputValidator.ValidateAndNormalizeAnswer(result.OutputJson, source.Snapshot.EvidenceKeys);
            await CompleteMessageAsync(lease, answerJson, result, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (AiServiceUnavailableException exception)
        {
            await FailMessageAsync(lease, CoachGenerationFailureKind.Configuration, exception.Message, cancellationToken);
        }
        catch (AiProviderException exception)
        {
            _logger.LogWarning(exception, "Coach message {MessageId} generation failed with provider failure kind {Kind}.",
                lease.Id, exception.Kind);
            await FailMessageAsync(lease, MapFailureKind(exception.Kind), exception.Message, cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Coach message {MessageId} generation failed unexpectedly.", lease.Id);
            await FailMessageAsync(lease, CoachGenerationFailureKind.ProviderFailure,
                "The coach response could not be generated. Please try again later.", cancellationToken);
        }

        return true;
    }

    private async Task<bool> ProcessDailyBriefingAsync(Guid briefingId, CancellationToken cancellationToken)
    {
        var lease = await ClaimDailyBriefingAsync(briefingId, cancellationToken);
        if (lease is null)
            return false;

        try
        {
            var source = await BuildDailySourceAsync(lease, cancellationToken);
            if (source is null)
                return true;
            var result = await GenerateAsync(CoachInteractionPromptCatalogue.DailyBriefingInstructions, source.Snapshot.SnapshotJson,
                CoachInteractionStructuredOutputSchema.DailyBriefingName, CoachInteractionStructuredOutputSchema.CreateDailyBriefing(),
                lease.UserId, CoachInteractionPromptCatalogue.DailyBriefingVersion, cancellationToken);
            var contentJson = _outputValidator.ValidateAndNormalizeDailyBriefing(result.OutputJson, source.Snapshot.EvidenceKeys);
            await CompleteDailyBriefingAsync(lease, contentJson, result, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (AiServiceUnavailableException exception)
        {
            await FailDailyBriefingAsync(lease, CoachGenerationFailureKind.Configuration, exception.Message, cancellationToken);
        }
        catch (AiProviderException exception)
        {
            _logger.LogWarning(exception, "Daily coach briefing {BriefingId} generation failed with provider failure kind {Kind}.",
                lease.Id, exception.Kind);
            await FailDailyBriefingAsync(lease, MapFailureKind(exception.Kind), exception.Message, cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Daily coach briefing {BriefingId} generation failed unexpectedly.", lease.Id);
            await FailDailyBriefingAsync(lease, CoachGenerationFailureKind.ProviderFailure,
                "The daily coach briefing could not be generated. Please try again later.", cancellationToken);
        }

        return true;
    }

    private async Task<CoachGenerationSourceResult?> BuildMessageSourceAsync(CoachGenerationLease lease,
        CancellationToken cancellationToken)
    {
        var message = await _context.CoachMessages.Include(candidate => candidate.Thread).FirstOrDefaultAsync(candidate =>
            candidate.Id == lease.Id && candidate.GenerationAttemptId == lease.AttemptId &&
            candidate.Status == CoachGenerationStatus.Processing, cancellationToken);
        if (message is null || message.ReplyToMessageId is null)
            return null;
        var question = await _context.CoachMessages.AsNoTracking().FirstOrDefaultAsync(candidate => candidate.Id ==
            message.ReplyToMessageId.Value && candidate.UserId == lease.UserId, cancellationToken);
        if (question?.Question is null)
            return null;
        var history = await LoadHistoryAsync(message.ThreadId, message.SequenceNumber, cancellationToken);
        var timeZoneId = string.IsNullOrWhiteSpace(question.TimeZoneId)
            ? throw new AiProviderException(AiProviderFailureKind.InvalidResponse, "The coach question timezone is unavailable.", false)
            : question.TimeZoneId;
        var snapshot = await _snapshotBuilder.BuildConversationAsync(lease.UserId, new CoachConversationContextRequest(question.Question,
            message.Thread.ContextSummary, history, timeZoneId, DateTime.UtcNow), cancellationToken);
        message.SetGenerationSource(lease.AttemptId, new CoachGenerationSource(snapshot.SourceFingerprint,
            CoachContextSnapshotVersions.Conversation, snapshot.SnapshotJson, CoachInteractionPromptCatalogue.ConversationVersion,
            CoachInteractionStructuredOutputSchema.ConversationVersion), DateTime.UtcNow);
        await SaveSourceAsync(cancellationToken);
        return new CoachGenerationSourceResult(snapshot);
    }

    private async Task<CoachGenerationSourceResult?> BuildDailySourceAsync(CoachGenerationLease lease,
        CancellationToken cancellationToken)
    {
        var briefing = await _context.DailyCoachBriefings.FirstOrDefaultAsync(candidate => candidate.Id == lease.Id &&
            candidate.GenerationAttemptId == lease.AttemptId && candidate.Status == CoachGenerationStatus.Processing, cancellationToken);
        if (briefing is null)
            return null;
        var snapshot = await _snapshotBuilder.BuildDailyBriefingAsync(lease.UserId,
            new CoachDailyBriefingContextRequest(briefing.TimeZoneId, DateTime.UtcNow), cancellationToken);
        briefing.SetGenerationSource(lease.AttemptId, new CoachGenerationSource(snapshot.SourceFingerprint,
            CoachContextSnapshotVersions.DailyBriefing, snapshot.SnapshotJson, CoachInteractionPromptCatalogue.DailyBriefingVersion,
            CoachInteractionStructuredOutputSchema.DailyBriefingVersion), DateTime.UtcNow);
        await SaveSourceAsync(cancellationToken);
        return new CoachGenerationSourceResult(snapshot);
    }

    private async Task<IReadOnlyList<CoachConversationHistoryMessage>> LoadHistoryAsync(Guid threadId, int beforeSequence,
        CancellationToken cancellationToken)
    {
        var messages = await _context.CoachMessages.AsNoTracking().Where(message => message.ThreadId == threadId &&
                message.SequenceNumber < beforeSequence && message.Status == CoachGenerationStatus.Completed)
            .OrderByDescending(message => message.SequenceNumber).Take(_options.ConversationContextMessageLimit).ToListAsync(cancellationToken);
        return messages.OrderBy(message => message.SequenceNumber).Select(ToHistoryMessage).Where(message => message is not null)
            .Select(message => message!).ToList();
    }

    private static CoachConversationHistoryMessage? ToHistoryMessage(CoachMessage message)
    {
        if (message.Role == CoachMessageRole.User && !string.IsNullOrWhiteSpace(message.Question))
            return new CoachConversationHistoryMessage("User", message.Question);
        if (message.Role != CoachMessageRole.Assistant || string.IsNullOrWhiteSpace(message.AnswerJson))
            return null;
        try
        {
            return JsonSerializer.Deserialize<CoachAnswerStructuredOutput>(message.AnswerJson, SerializerOptions) is { } answer
                ? new CoachConversationHistoryMessage("Assistant", answer.AnswerMarkdown) : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private async Task<StructuredAiGenerationResult> GenerateAsync(string instructions, string snapshotJson, string schemaName,
        JsonElement schema, Guid userId, string promptVersion, CancellationToken cancellationToken)
    {
        for (var attempt = 1; ; attempt++)
        {
            try
            {
                return await _aiClient.GenerateStructuredAsync(new StructuredAiGenerationRequest(instructions, snapshotJson,
                    schemaName, schema, AiSafetyIdentifier.FromUserId(userId), promptVersion), cancellationToken);
            }
            catch (AiProviderException exception) when (exception.IsRetryable && attempt < MaximumTransientAttempts)
            {
                var delay = exception.RetryAfter ?? TimeSpan.FromSeconds(attempt);
                await Task.Delay(delay > TimeSpan.FromSeconds(30) ? TimeSpan.FromSeconds(30) : delay, cancellationToken);
            }
        }
    }

    private async Task<CoachGenerationLease?> ClaimMessageAsync(Guid messageId, CancellationToken cancellationToken)
    {
        var candidate = await _context.CoachMessages.FirstOrDefaultAsync(message => message.Id == messageId &&
            message.Status == CoachGenerationStatus.Pending, cancellationToken);
        return candidate is null || !candidate.TryClaim(candidate.GenerationAttemptId,
            DateTime.UtcNow.AddSeconds(_options.ProcessingLeaseSeconds), DateTime.UtcNow)
            ? null : await SaveClaimAsync(candidate.Id, candidate.UserId, candidate.GenerationAttemptId, cancellationToken);
    }

    private async Task<CoachGenerationLease?> ClaimDailyBriefingAsync(Guid briefingId, CancellationToken cancellationToken)
    {
        var candidate = await _context.DailyCoachBriefings.FirstOrDefaultAsync(briefing => briefing.Id == briefingId &&
            briefing.Status == CoachGenerationStatus.Pending, cancellationToken);
        return candidate is null || !candidate.TryClaim(candidate.GenerationAttemptId,
            DateTime.UtcNow.AddSeconds(_options.ProcessingLeaseSeconds), DateTime.UtcNow)
            ? null : await SaveClaimAsync(candidate.Id, candidate.UserId, candidate.GenerationAttemptId, cancellationToken);
    }

    private async Task<CoachGenerationLease?> SaveClaimAsync(Guid id, Guid userId, Guid attemptId,
        CancellationToken cancellationToken)
    {
        try
        {
            await _context.SaveChangesAsync(cancellationToken);
            return new CoachGenerationLease(id, userId, attemptId);
        }
        catch (DbUpdateConcurrencyException)
        {
            _context.ChangeTracker.Clear();
            return null;
        }
    }

    private async Task CompleteMessageAsync(CoachGenerationLease lease, string answerJson, StructuredAiGenerationResult result,
        CancellationToken cancellationToken)
    {
        _context.ChangeTracker.Clear();
        var message = await _context.CoachMessages.Include(candidate => candidate.Thread).FirstOrDefaultAsync(candidate =>
            candidate.Id == lease.Id && candidate.GenerationAttemptId == lease.AttemptId &&
            candidate.Status == CoachGenerationStatus.Processing, cancellationToken);
        if (message is null)
            return;
        message.Complete(lease.AttemptId, answerJson, Completion(result), DateTime.UtcNow);
        var summary = JsonSerializer.Deserialize<CoachAnswerStructuredOutput>(answerJson, SerializerOptions)?.UpdatedThreadSummary;
        if (!string.IsNullOrWhiteSpace(summary))
            message.Thread.TryUpdateContextSummary(message.SequenceNumber, summary, DateTime.UtcNow);
        await SaveCompletionAsync(lease, cancellationToken);
    }

    private async Task CompleteDailyBriefingAsync(CoachGenerationLease lease, string contentJson, StructuredAiGenerationResult result,
        CancellationToken cancellationToken)
    {
        _context.ChangeTracker.Clear();
        var briefing = await _context.DailyCoachBriefings.FirstOrDefaultAsync(candidate => candidate.Id == lease.Id &&
            candidate.GenerationAttemptId == lease.AttemptId && candidate.Status == CoachGenerationStatus.Processing, cancellationToken);
        if (briefing is null)
            return;
        briefing.Complete(lease.AttemptId, contentJson, Completion(result), DateTime.UtcNow);
        await SaveCompletionAsync(lease, cancellationToken);
    }

    private async Task FailMessageAsync(CoachGenerationLease lease, CoachGenerationFailureKind failureKind, string message,
        CancellationToken cancellationToken)
    {
        _context.ChangeTracker.Clear();
        var current = await _context.CoachMessages.FirstOrDefaultAsync(candidate => candidate.Id == lease.Id &&
            candidate.GenerationAttemptId == lease.AttemptId && candidate.Status == CoachGenerationStatus.Processing, cancellationToken);
        if (current is null)
            return;
        current.Fail(lease.AttemptId, failureKind, message, DateTime.UtcNow);
        await SaveCompletionAsync(lease, cancellationToken);
    }

    private async Task FailDailyBriefingAsync(CoachGenerationLease lease, CoachGenerationFailureKind failureKind, string message,
        CancellationToken cancellationToken)
    {
        _context.ChangeTracker.Clear();
        var current = await _context.DailyCoachBriefings.FirstOrDefaultAsync(candidate => candidate.Id == lease.Id &&
            candidate.GenerationAttemptId == lease.AttemptId && candidate.Status == CoachGenerationStatus.Processing, cancellationToken);
        if (current is null)
            return;
        current.Fail(lease.AttemptId, failureKind, message, DateTime.UtcNow);
        await SaveCompletionAsync(lease, cancellationToken);
    }

    private async Task SaveSourceAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            _context.ChangeTracker.Clear();
            throw new AiProviderException(AiProviderFailureKind.ProviderFailure, "The coach generation was superseded.", false);
        }
    }

    private async Task SaveCompletionAsync(CoachGenerationLease lease, CancellationToken cancellationToken)
    {
        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            _logger.LogInformation("Coach generation {GenerationId} attempt {AttemptId} was superseded before save.", lease.Id,
                lease.AttemptId);
        }
    }

    private async Task RequeueExpiredClaimsAsync(CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var message = await _context.CoachMessages.FirstOrDefaultAsync(candidate => candidate.Status == CoachGenerationStatus.Processing &&
            candidate.ProcessingLeaseExpiresAt <= now, cancellationToken);
        if (message?.RequeueExpiredClaim(now) == true)
            await _context.SaveChangesAsync(cancellationToken);
        var briefing = await _context.DailyCoachBriefings.FirstOrDefaultAsync(candidate => candidate.Status == CoachGenerationStatus.Processing &&
            candidate.ProcessingLeaseExpiresAt <= now, cancellationToken);
        if (briefing?.RequeueExpiredClaim(now) == true)
            await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task<CoachGenerationWork?> FindNextPendingWorkAsync(CancellationToken cancellationToken)
    {
        var message = await _context.CoachMessages.AsNoTracking().Where(candidate => candidate.Status == CoachGenerationStatus.Pending)
            .OrderBy(candidate => candidate.RequestedAt).Select(candidate => new { candidate.Id, candidate.RequestedAt })
            .FirstOrDefaultAsync(cancellationToken);
        var briefing = await _context.DailyCoachBriefings.AsNoTracking().Where(candidate => candidate.Status == CoachGenerationStatus.Pending)
            .OrderBy(candidate => candidate.RequestedAt).Select(candidate => new { candidate.Id, candidate.RequestedAt })
            .FirstOrDefaultAsync(cancellationToken);
        if (message is null && briefing is null)
            return null;
        return briefing is null || message is not null && message.RequestedAt <= briefing.RequestedAt
            ? new CoachGenerationWork.Message(message!.Id) : new CoachGenerationWork.DailyBriefing(briefing.Id);
    }

    private static CoachGenerationCompletion Completion(StructuredAiGenerationResult result) =>
        new("OpenAI", result.ProviderResponseId, result.Model, result.InputTokens, result.OutputTokens, result.TotalTokens);

    private static CoachGenerationFailureKind MapFailureKind(AiProviderFailureKind kind) => kind switch
    {
        AiProviderFailureKind.Authentication => CoachGenerationFailureKind.Authentication,
        AiProviderFailureKind.RateLimited => CoachGenerationFailureKind.RateLimited,
        AiProviderFailureKind.Timeout => CoachGenerationFailureKind.Timeout,
        AiProviderFailureKind.Network => CoachGenerationFailureKind.Network,
        AiProviderFailureKind.Refusal => CoachGenerationFailureKind.Refusal,
        AiProviderFailureKind.IncompleteResponse => CoachGenerationFailureKind.IncompleteResponse,
        AiProviderFailureKind.InvalidResponse => CoachGenerationFailureKind.InvalidResponse,
        _ => CoachGenerationFailureKind.ProviderFailure
    };

    private sealed record CoachGenerationLease(Guid Id, Guid UserId, Guid AttemptId);
    private sealed record CoachGenerationSourceResult(CoachContextSnapshotBuildResult Snapshot);
    private abstract record CoachGenerationWork
    {
        public sealed record Message(Guid Id) : CoachGenerationWork;
        public sealed record DailyBriefing(Guid Id) : CoachGenerationWork;
    }
}
