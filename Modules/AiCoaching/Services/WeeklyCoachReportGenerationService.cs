using backend.Data;
using backend.Modules.AiCoaching.Configuration;
using backend.Modules.AiCoaching.Contracts;
using backend.Modules.AiCoaching.Domain;
using backend.Modules.Shared.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace backend.Modules.AiCoaching.Services;

public interface IWeeklyCoachReportGenerationService
{
    Task<bool> ProcessOneAsync(CancellationToken cancellationToken);
}

public sealed class WeeklyCoachReportGenerationService : IWeeklyCoachReportGenerationService
{
    private const int MaximumTransientAttempts = 2;
    private readonly FitspireDbContext _context;
    private readonly IGenerativeAiClient _aiClient;
    private readonly IWeeklyCoachReportOutputValidator _outputValidator;
    private readonly OpenAiOptions _options;
    private readonly ILogger<WeeklyCoachReportGenerationService> _logger;

    public WeeklyCoachReportGenerationService(FitspireDbContext context, IGenerativeAiClient aiClient,
        IWeeklyCoachReportOutputValidator outputValidator, IOptions<OpenAiOptions> options,
        ILogger<WeeklyCoachReportGenerationService> logger)
    {
        _context = context;
        _aiClient = aiClient;
        _outputValidator = outputValidator;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<bool> ProcessOneAsync(CancellationToken cancellationToken)
    {
        if (!_options.Enabled)
            return false;

        var lease = await ClaimNextAsync(cancellationToken);
        if (lease is null)
            return false;

        try
        {
            var result = await GenerateWithRetryAsync(lease, cancellationToken);
            var reportJson = _outputValidator.ValidateAndNormalize(result.OutputJson, lease.SnapshotJson);
            await CompleteAsync(lease, reportJson, result, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (AiServiceUnavailableException exception)
        {
            await FailAsync(lease, WeeklyCoachGenerationFailureKind.Configuration, exception.Message, cancellationToken);
        }
        catch (AiProviderException exception)
        {
            await FailAsync(lease, MapFailureKind(exception.Kind), exception.Message, cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Weekly coach report {ReportId} generation failed unexpectedly.", lease.ReportId);
            await FailAsync(lease, WeeklyCoachGenerationFailureKind.ProviderFailure,
                "The coaching report could not be generated. Please try again later.", cancellationToken);
        }

        return true;
    }

    private async Task<WeeklyCoachGenerationLease?> ClaimNextAsync(CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        await RequeueExpiredClaimAsync(now, cancellationToken);
        var candidate = await _context.WeeklyCoachReports.OrderBy(report => report.RequestedAt)
            .FirstOrDefaultAsync(report => report.Status == WeeklyCoachReportStatus.Pending, cancellationToken);
        if (candidate is null || !candidate.TryClaim(candidate.GenerationAttemptId,
                now.AddSeconds(_options.ProcessingLeaseSeconds), now))
        {
            return null;
        }

        try
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return new WeeklyCoachGenerationLease(candidate.Id, candidate.UserId, candidate.GenerationAttemptId,
                candidate.SnapshotJson, candidate.PromptVersion);
        }
        catch (DbUpdateConcurrencyException)
        {
            return null;
        }
    }

    private async Task RequeueExpiredClaimAsync(DateTime now, CancellationToken cancellationToken)
    {
        var expired = await _context.WeeklyCoachReports.Where(report => report.Status == WeeklyCoachReportStatus.Processing &&
                report.ProcessingLeaseExpiresAt <= now).OrderBy(report => report.ProcessingLeaseExpiresAt).FirstOrDefaultAsync(cancellationToken);
        if (expired is null || !expired.RequeueExpiredClaim(now))
            return;

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            _context.ChangeTracker.Clear();
        }
    }

    private async Task<StructuredAiGenerationResult> GenerateWithRetryAsync(WeeklyCoachGenerationLease lease,
        CancellationToken cancellationToken)
    {
        for (var attempt = 1; ; attempt++)
        {
            try
            {
                return await _aiClient.GenerateStructuredAsync(new StructuredAiGenerationRequest(
                    WeeklyCoachPromptCatalogue.Instructions, lease.SnapshotJson, WeeklyCoachStructuredOutputSchema.Name,
                    WeeklyCoachStructuredOutputSchema.Create(), AiSafetyIdentifier.FromUserId(lease.UserId), lease.PromptVersion), cancellationToken);
            }
            catch (AiProviderException exception) when (exception.IsRetryable && attempt < MaximumTransientAttempts)
            {
                _logger.LogWarning("Weekly coach report {ReportId} provider attempt {Attempt} failed and will retry. Kind: {Kind}",
                    lease.ReportId, attempt, exception.Kind);
                var delay = exception.RetryAfter ?? TimeSpan.FromSeconds(attempt);
                await Task.Delay(delay > TimeSpan.FromSeconds(30) ? TimeSpan.FromSeconds(30) : delay, cancellationToken);
            }
        }
    }

    private async Task CompleteAsync(WeeklyCoachGenerationLease lease, string reportJson,
        StructuredAiGenerationResult result, CancellationToken cancellationToken)
    {
        var report = await FindCurrentAttemptAsync(lease, cancellationToken);
        if (report is null)
            return;

        report.Complete(lease.AttemptId, reportJson, new WeeklyCoachCompletion("OpenAI", result.ProviderResponseId,
            result.Model, result.InputTokens, result.OutputTokens, result.TotalTokens), DateTime.UtcNow);
        await SaveCurrentAttemptAsync(lease, cancellationToken);
    }

    private async Task FailAsync(WeeklyCoachGenerationLease lease, WeeklyCoachGenerationFailureKind failureKind,
        string safeMessage, CancellationToken cancellationToken)
    {
        var report = await FindCurrentAttemptAsync(lease, cancellationToken);
        if (report is null)
            return;

        report.Fail(lease.AttemptId, failureKind, safeMessage, DateTime.UtcNow);
        await SaveCurrentAttemptAsync(lease, cancellationToken);
    }

    private Task<WeeklyCoachReport?> FindCurrentAttemptAsync(WeeklyCoachGenerationLease lease,
        CancellationToken cancellationToken) => _context.WeeklyCoachReports.FirstOrDefaultAsync(report =>
        report.Id == lease.ReportId && report.GenerationAttemptId == lease.AttemptId &&
        report.Status == WeeklyCoachReportStatus.Processing, cancellationToken);

    private async Task SaveCurrentAttemptAsync(WeeklyCoachGenerationLease lease, CancellationToken cancellationToken)
    {
        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            _logger.LogInformation("Weekly coach report {ReportId} attempt {AttemptId} was superseded before it could be saved.",
                lease.ReportId, lease.AttemptId);
        }
    }

    private static WeeklyCoachGenerationFailureKind MapFailureKind(AiProviderFailureKind kind) => kind switch
    {
        AiProviderFailureKind.Authentication => WeeklyCoachGenerationFailureKind.Authentication,
        AiProviderFailureKind.RateLimited => WeeklyCoachGenerationFailureKind.RateLimited,
        AiProviderFailureKind.Timeout => WeeklyCoachGenerationFailureKind.Timeout,
        AiProviderFailureKind.Network => WeeklyCoachGenerationFailureKind.Network,
        AiProviderFailureKind.Refusal => WeeklyCoachGenerationFailureKind.Refusal,
        AiProviderFailureKind.IncompleteResponse => WeeklyCoachGenerationFailureKind.IncompleteResponse,
        AiProviderFailureKind.InvalidResponse => WeeklyCoachGenerationFailureKind.InvalidResponse,
        _ => WeeklyCoachGenerationFailureKind.ProviderFailure
    };
}

public sealed record WeeklyCoachGenerationLease(Guid ReportId, Guid UserId, Guid AttemptId, string SnapshotJson,
    string PromptVersion);
