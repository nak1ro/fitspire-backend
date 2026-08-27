using System.Data;
using backend.Data;
using backend.Modules.AiCoaching.Configuration;
using backend.Modules.AiCoaching.Contracts;
using backend.Modules.AiCoaching.Domain;
using backend.Modules.Shared.Domain;
using backend.Modules.Workout.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Npgsql;

namespace backend.Modules.AiCoaching.Services;

public interface ICoachInteractionQueueService
{
    Task<CoachQueuedExchange> QueueQuestionAsync(Guid userId, Guid threadId, Guid clientRequestId, string question,
        CancellationToken cancellationToken);
    Task<CoachQueuedDailyBriefing> QueueDailyBriefingAsync(Guid userId, CancellationToken cancellationToken);
    Task RetryMessageAsync(Guid userId, Guid threadId, Guid messageId, CancellationToken cancellationToken);
    Task RetryDailyBriefingAsync(Guid userId, Guid briefingId, CancellationToken cancellationToken);
    Task RegenerateDailyBriefingAsync(Guid userId, Guid briefingId, CancellationToken cancellationToken);
    Task ScheduleDueDailyBriefingsAsync(DateTime utcNow, CancellationToken cancellationToken);
}

public sealed class CoachInteractionQueueService : ICoachInteractionQueueService
{
    private readonly FitspireDbContext _context;
    private readonly ICoachUserTimeZoneService _timeZoneService;
    private readonly OpenAiOptions _openAiOptions;
    private readonly AiCoachInteractionOptions _options;

    public CoachInteractionQueueService(FitspireDbContext context, ICoachUserTimeZoneService timeZoneService,
        IOptions<OpenAiOptions> openAiOptions, IOptions<AiCoachInteractionOptions> options)
    {
        _context = context;
        _timeZoneService = timeZoneService;
        _openAiOptions = openAiOptions.Value;
        _options = options.Value;
    }

    public async Task<CoachQueuedExchange> QueueQuestionAsync(Guid userId, Guid threadId, Guid clientRequestId,
        string question, CancellationToken cancellationToken)
    {
        EnsureAiAvailable();
        var now = DateTime.UtcNow;
        var timeZoneId = await _timeZoneService.GetAsync(userId, cancellationToken);
        var localDate = CoachLocalDate.Resolve(timeZoneId, now);
        return await QueueQuestionWithRetryAsync(userId, threadId, clientRequestId, question, localDate, timeZoneId, now,
            cancellationToken);
    }

    public async Task<CoachQueuedDailyBriefing> QueueDailyBriefingAsync(Guid userId, CancellationToken cancellationToken)
    {
        EnsureAiAvailable();
        var now = DateTime.UtcNow;
        var timeZoneId = await _timeZoneService.GetAsync(userId, cancellationToken);
        var localDate = CoachLocalDate.Resolve(timeZoneId, now);
        return await QueueDailyWithRetryAsync(userId, localDate, timeZoneId, now, cancellationToken);
    }

    public async Task RetryMessageAsync(Guid userId, Guid threadId, Guid messageId, CancellationToken cancellationToken)
    {
        EnsureAiAvailable();
        await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        var message = await _context.CoachMessages.FirstOrDefaultAsync(candidate => candidate.Id == messageId &&
            candidate.ThreadId == threadId && candidate.UserId == userId, cancellationToken)
            ?? throw new NotFoundException("Coach message was not found.");
        await EnsureThreadCanReceiveQuestionAsync(threadId, messageId, cancellationToken);
        message.Retry(DateTime.UtcNow);
        await _context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    public async Task RetryDailyBriefingAsync(Guid userId, Guid briefingId, CancellationToken cancellationToken)
    {
        EnsureAiAvailable();
        var timeZoneId = await _timeZoneService.GetAsync(userId, cancellationToken);
        var localDate = CoachLocalDate.Resolve(timeZoneId, DateTime.UtcNow);
        var briefing = await _context.DailyCoachBriefings.FirstOrDefaultAsync(candidate => candidate.Id == briefingId &&
            candidate.UserId == userId, cancellationToken) ?? throw new NotFoundException("Daily coach briefing was not found.");
        if (briefing.LocalDate != localDate)
            throw new DomainException("Only today's daily coach briefing can be retried.");
        briefing.Retry(DateTime.UtcNow);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task RegenerateDailyBriefingAsync(Guid userId, Guid briefingId, CancellationToken cancellationToken)
    {
        EnsureAiAvailable();
        var now = DateTime.UtcNow;
        var timeZoneId = await _timeZoneService.GetAsync(userId, cancellationToken);
        var localDate = CoachLocalDate.Resolve(timeZoneId, now);
        await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        var briefing = await _context.DailyCoachBriefings.FirstOrDefaultAsync(candidate =>
            candidate.Id == briefingId && candidate.UserId == userId, cancellationToken)
            ?? throw new NotFoundException("Daily coach briefing was not found.");
        if (briefing.LocalDate != localDate)
            throw new DomainException("Only today's daily coach briefing can be regenerated.");

        briefing.Regenerate(now);
        await _context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    public async Task ScheduleDueDailyBriefingsAsync(DateTime utcNow, CancellationToken cancellationToken)
    {
        EnsureAiAvailable();
        var users = await _context.Users.AsNoTracking().Include(user => user.AppUserPreference).ToListAsync(cancellationToken);
        foreach (var user in users)
        {
            var timeZoneId = string.IsNullOrWhiteSpace(user.AppUserPreference?.TimeZoneId) ? "Central European Standard Time" : user.AppUserPreference.TimeZoneId;
            var localDate = CoachLocalDate.Resolve(timeZoneId, utcNow);
            var localHour = TimeZoneInfo.ConvertTimeFromUtc(utcNow, TimeZoneInfo.FindSystemTimeZoneById(timeZoneId)).Hour;
            if (localHour < _options.DailyBriefingLocalHour)
                continue;
            await ScheduleUserDailyBriefingAsync(user.Id, localDate, timeZoneId, utcNow, cancellationToken);
        }
    }

    private async Task ScheduleUserDailyBriefingAsync(Guid userId, DateOnly localDate, string timeZoneId, DateTime utcNow,
        CancellationToken cancellationToken)
    {
        var briefing = await _context.DailyCoachBriefings.FirstOrDefaultAsync(candidate => candidate.UserId == userId &&
            candidate.LocalDate == localDate, cancellationToken);
        if (briefing is null)
        {
            await QueueDailyWithRetryAsync(userId, localDate, timeZoneId, utcNow, cancellationToken);
            return;
        }
        if (briefing.CompletedAt is not { } completedAt || !await HasMeaningfulActivityAfterAsync(userId, completedAt, cancellationToken))
            return;

        await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        var current = await _context.DailyCoachBriefings.FirstOrDefaultAsync(candidate => candidate.Id == briefing.Id,
            cancellationToken);
        if (current?.TryRefreshAfterActivity(utcNow) == true)
            await _context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    private async Task<bool> HasMeaningfulActivityAfterAsync(Guid userId, DateTime afterUtc, CancellationToken cancellationToken)
    {
        var completedWorkout = await _context.UserWorkouts.AnyAsync(workout => workout.UserId == userId &&
            workout.Status == WorkoutStatus.Completed && workout.CompletedAt > afterUtc, cancellationToken);
        if (completedWorkout) return true;
        var mealChange = await _context.Meals.AnyAsync(meal => meal.UserId == userId && meal.DeletedAt == null &&
            (meal.CreatedAt > afterUtc || meal.UpdatedAt > afterUtc), cancellationToken);
        return mealChange || await _context.Goals.AnyAsync(goal => goal.UserId == userId &&
            (goal.CreatedAt > afterUtc || goal.UpdatedAt > afterUtc), cancellationToken);
    }

    private async Task<CoachQueuedExchange> QueueQuestionWithRetryAsync(Guid userId, Guid threadId, Guid clientRequestId,
        string question, DateOnly localDate, string timeZoneId, DateTime now, CancellationToken cancellationToken)
    {
        for (var attempt = 1; attempt <= 2; attempt++)
        {
            try
            {
                return await QueueQuestionOnceAsync(userId, threadId, clientRequestId, question, localDate, timeZoneId, now,
                    cancellationToken);
            }
            catch (DbUpdateException exception) when (IsSerializationFailure(exception) && attempt == 1)
            {
                _context.ChangeTracker.Clear();
            }
            catch (DbUpdateException exception) when (IsSerializationFailure(exception))
            {
                throw new ConflictException("Coach question submission conflicted. Please try again.");
            }
        }

        throw new ConflictException("Coach question submission conflicted. Please try again.");
    }

    private async Task<CoachQueuedExchange> QueueQuestionOnceAsync(Guid userId, Guid threadId, Guid clientRequestId,
        string question, DateOnly localDate, string timeZoneId, DateTime now, CancellationToken cancellationToken)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        try
        {
            var existing = await FindExistingExchangeAsync(userId, clientRequestId, cancellationToken);
            if (existing is not null)
            {
                await transaction.CommitAsync(cancellationToken);
                return existing;
            }

            var thread = await _context.CoachThreads.FirstOrDefaultAsync(candidate => candidate.Id == threadId &&
                candidate.UserId == userId, cancellationToken) ?? throw new NotFoundException("Coach thread was not found.");
            await EnsureThreadCanReceiveQuestionAsync(threadId, cancellationToken);
            await EnsureWithinDailyQuestionLimitAsync(userId, localDate, timeZoneId, cancellationToken);

            var userMessage = CoachMessage.CreateUserQuestion(Guid.NewGuid(), threadId, userId,
                thread.ReserveNextSequenceNumber(now), clientRequestId, question, localDate, timeZoneId, now);
            var assistantMessage = CoachMessage.CreatePendingAssistant(Guid.NewGuid(), userMessage,
                thread.ReserveNextSequenceNumber(now), now);
            thread.ApplyAutomaticTitle(question, now);
            _context.CoachMessages.AddRange(userMessage, assistantMessage);
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return new CoachQueuedExchange(userMessage.Id, assistantMessage.Id, assistantMessage.Status, true);
        }
        catch (DbUpdateException exception) when (IsUniqueViolation(exception))
        {
            await transaction.RollbackAsync(cancellationToken);
            _context.ChangeTracker.Clear();
            return await FindExistingExchangeAsync(userId, clientRequestId, cancellationToken)
                   ?? throw new ConflictException("Coach question submission conflicted. Please try again.");
        }
    }

    private async Task<CoachQueuedDailyBriefing> QueueDailyWithRetryAsync(Guid userId, DateOnly localDate, string timeZoneId,
        DateTime now, CancellationToken cancellationToken)
    {
        for (var attempt = 1; attempt <= 2; attempt++)
        {
            try
            {
                return await QueueDailyOnceAsync(userId, localDate, timeZoneId, now, cancellationToken);
            }
            catch (DbUpdateException exception) when (IsSerializationFailure(exception) && attempt == 1)
            {
                _context.ChangeTracker.Clear();
            }
            catch (DbUpdateException exception) when (IsSerializationFailure(exception))
            {
                throw new ConflictException("Daily coach briefing submission conflicted. Please try again.");
            }
        }

        throw new ConflictException("Daily coach briefing submission conflicted. Please try again.");
    }

    private async Task<CoachQueuedDailyBriefing> QueueDailyOnceAsync(Guid userId, DateOnly localDate, string timeZoneId,
        DateTime now, CancellationToken cancellationToken)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        try
        {
            var existing = await _context.DailyCoachBriefings.FirstOrDefaultAsync(candidate => candidate.UserId == userId &&
                candidate.LocalDate == localDate, cancellationToken);
            if (existing is not null)
            {
                await transaction.CommitAsync(cancellationToken);
                return new CoachQueuedDailyBriefing(existing.Id, existing.Status, false);
            }

            var briefing = DailyCoachBriefing.CreatePending(Guid.NewGuid(), userId, localDate, timeZoneId, now);
            _context.DailyCoachBriefings.Add(briefing);
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return new CoachQueuedDailyBriefing(briefing.Id, briefing.Status, true);
        }
        catch (DbUpdateException exception) when (IsUniqueViolation(exception))
        {
            await transaction.RollbackAsync(cancellationToken);
            _context.ChangeTracker.Clear();
            var existing = await _context.DailyCoachBriefings.AsNoTracking().FirstOrDefaultAsync(candidate =>
                candidate.UserId == userId && candidate.LocalDate == localDate, cancellationToken);
            return existing is null
                ? throw new ConflictException("Daily coach briefing submission conflicted. Please try again.")
                : new CoachQueuedDailyBriefing(existing.Id, existing.Status, false);
        }
    }

    private async Task<CoachQueuedExchange?> FindExistingExchangeAsync(Guid userId, Guid clientRequestId,
        CancellationToken cancellationToken)
    {
        var question = await _context.CoachMessages.AsNoTracking().FirstOrDefaultAsync(candidate => candidate.UserId == userId &&
            candidate.ClientRequestId == clientRequestId, cancellationToken);
        if (question is null)
            return null;
        var response = await _context.CoachMessages.AsNoTracking().FirstOrDefaultAsync(candidate =>
            candidate.ReplyToMessageId == question.Id, cancellationToken);
        return response is null ? null : new CoachQueuedExchange(question.Id, response.Id, response.Status, false);
    }

    private Task EnsureThreadCanReceiveQuestionAsync(Guid threadId, CancellationToken cancellationToken) =>
        EnsureThreadCanReceiveQuestionAsync(threadId, null, cancellationToken);

    private async Task EnsureThreadCanReceiveQuestionAsync(Guid threadId, Guid? excludedMessageId,
        CancellationToken cancellationToken)
    {
        var hasActiveResponse = await _context.CoachMessages.AnyAsync(candidate => candidate.ThreadId == threadId &&
            (excludedMessageId == null || candidate.Id != excludedMessageId.Value) &&
            candidate.Role == CoachMessageRole.Assistant && (candidate.Status == CoachGenerationStatus.Pending ||
            candidate.Status == CoachGenerationStatus.Processing), cancellationToken);
        if (hasActiveResponse)
            throw new ConflictException("Wait for the current coach response before asking another question in this thread.");
    }

    private async Task EnsureWithinDailyQuestionLimitAsync(Guid userId, DateOnly localDate, string timeZoneId,
        CancellationToken cancellationToken)
    {
        var count = await _context.CoachMessages.CountAsync(candidate => candidate.UserId == userId &&
            candidate.Role == CoachMessageRole.User && candidate.LocalRequestDate == localDate, cancellationToken);
        if (count >= _options.DailyQuestionLimit)
            throw new CoachQuestionQuotaExceededException("You have reached today's coach question limit.",
                CoachLocalDate.NextStartUtc(timeZoneId, localDate));
    }

    private void EnsureAiAvailable()
    {
        if (!_openAiOptions.Enabled || string.IsNullOrWhiteSpace(_openAiOptions.ApiKey))
            throw new AiServiceUnavailableException("AI coaching is not configured.");
    }

    private static bool IsUniqueViolation(DbUpdateException exception) =>
        exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation };

    private static bool IsSerializationFailure(DbUpdateException exception) =>
        exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.SerializationFailure };
}

public sealed record CoachQueuedExchange(Guid UserMessageId, Guid AssistantMessageId, CoachGenerationStatus Status, bool Accepted);

public sealed record CoachQueuedDailyBriefing(Guid BriefingId, CoachGenerationStatus Status, bool Accepted);

public sealed class CoachQuestionQuotaExceededException : Exception
{
    public CoachQuestionQuotaExceededException(string message, DateTime resetAtUtc) : base(message)
    {
        ResetAtUtc = resetAtUtc;
    }

    public DateTime ResetAtUtc { get; }
}
