using backend.Data;
using backend.Modules.Badge.Services;
using backend.Modules.Goal.Domain.Entities;
using backend.Modules.Goal.Domain.Enums;
using backend.Modules.Notification.Domain.Constants;
using backend.Modules.Notification.Domain.Enums;
using backend.Modules.Notification.Services;
using backend.Modules.Progress.Services;
using backend.Modules.Shared;
using Microsoft.EntityFrameworkCore;

namespace backend.Modules.Goal.Services;

public interface IGoalProgressService
{
    Task<IReadOnlyList<Guid>> RecalculateForUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task ProcessDuePeriodsAsync(DateTime nowUtc, CancellationToken cancellationToken = default);
}

public class GoalProgressService : IGoalProgressService
{
    private readonly FitspireDbContext _context;
    private readonly INotificationService _notifications;
    private readonly IBadgeEvaluationService _badges;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IGoalTransactionService _transactions;

    public GoalProgressService(FitspireDbContext context, INotificationService notifications, IBadgeEvaluationService badges,
        IUnitOfWork unitOfWork, IGoalTransactionService transactions)
    {
        _context = context;
        _notifications = notifications;
        _badges = badges;
        _unitOfWork = unitOfWork;
        _transactions = transactions;
    }

    public async Task<IReadOnlyList<Guid>> RecalculateForUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var completedPeriodIds = new List<Guid>();
        var periods = await _context.GoalPeriods.Include(period => period.Goal).ThenInclude(goal => goal.GoalType)
            .Where(period => period.Goal.UserId == userId && period.Goal.Status == GoalStatus.Active && period.Status == "Active")
            .OrderBy(period => period.StartAt)
            .ToListAsync(cancellationToken);
        foreach (var period in periods)
        {
            var goal = period.Goal;
            if (string.IsNullOrWhiteSpace(goal.GoalType.MetricCode))
                continue;
            var progress = await CalculateProgressAsync(goal, period, cancellationToken);
            var previousGoalValue = goal.CurrentValue;
            var wasActive = period.Status == "Active";
            var changed = period.SetProgress(progress);
            goal.ApplyCurrentPeriodProgress(period.ProgressValue, period.Status == "Completed");
            if (changed)
                await _context.GoalProgressEntries.AddAsync(new GoalProgressEntry(Guid.NewGuid(), goal.Id, previousGoalValue,
                    goal.CurrentValue, "recalculation"), cancellationToken);
            if (wasActive && period.Status == "Completed")
            {
                await CreateCompletionNotificationAsync(goal, cancellationToken);
                completedPeriodIds.Add(period.Id);
            }
        }

        return completedPeriodIds;
    }

    private async Task<double> CalculateProgressAsync(UserGoal goal, GoalPeriod period, CancellationToken cancellationToken)
    {
        var periodStart = period.StartAt > goal.StartDate ? period.StartAt : goal.StartDate;
        var query = _context.ActivityContributions.Where(contribution => contribution.UserId == goal.UserId && contribution.IsActive &&
            contribution.MetricCode == goal.GoalType.MetricCode && contribution.OccurredAt >= periodStart && contribution.OccurredAt < period.EndAt);
        if (!string.IsNullOrWhiteSpace(goal.SelectedWorkoutType))
            query = query.Where(contribution => contribution.WorkoutType == goal.SelectedWorkoutType);
        if (goal.SelectedExerciseId.HasValue)
            query = query.Where(contribution => contribution.ExerciseId == goal.SelectedExerciseId);

        if (goal.GoalType.MeasurementType == GoalMeasurementType.Streak)
        {
            var occurredAt = await query.Select(contribution => contribution.OccurredAt).ToListAsync(cancellationToken);
            return WorkoutStreakCalculator.GetLongestStreakDays(occurredAt, goal.TimeZoneId);
        }

        var values = await query.Select(contribution => contribution.Value).ToListAsync(cancellationToken);
        return goal.GoalType.MeasurementType == GoalMeasurementType.SingleEvent ? values.DefaultIfEmpty(0).Max() : values.Sum();
    }

    private Task CreateCompletionNotificationAsync(UserGoal goal, CancellationToken cancellationToken) =>
        _notifications.CreateAsync(goal.UserId, NotificationType.GoalCompleted, $"You completed your goal: {goal.GoalType.Name}.",
            referenceEntityId: goal.Id, referenceEntityType: NotificationReferenceTypes.Goal, cancellationToken: cancellationToken);

    public async Task ProcessDuePeriodsAsync(DateTime nowUtc, CancellationToken cancellationToken = default)
    {
        await _transactions.ExecuteAsync(async token =>
        {
            var completedPeriods = await RecalculateDueUsersAsync(nowUtc, token);
            await _unitOfWork.SaveChangesAsync(token);
            await EvaluateCompletedPeriodsAsync(completedPeriods, token);
            await _unitOfWork.SaveChangesAsync(token);
            await CloseDuePeriodsAsync(nowUtc, token);
            await _unitOfWork.SaveChangesAsync(token);
            return true;
        }, cancellationToken);
    }

    private async Task<IReadOnlyDictionary<Guid, IReadOnlyList<Guid>>> RecalculateDueUsersAsync(DateTime nowUtc,
        CancellationToken cancellationToken)
    {
        var userIds = await _context.GoalPeriods.Where(period => period.Goal.Status == GoalStatus.Active && period.Status == "Active" && period.EndAt <= nowUtc)
            .Select(period => period.Goal.UserId).Distinct().ToListAsync(cancellationToken);
        var completedPeriods = new Dictionary<Guid, IReadOnlyList<Guid>>();
        foreach (var userId in userIds)
        {
            var periods = await RecalculateForUserAsync(userId, cancellationToken);
            if (periods.Count > 0)
                completedPeriods[userId] = periods;
        }

        return completedPeriods;
    }

    private async Task EvaluateCompletedPeriodsAsync(IReadOnlyDictionary<Guid, IReadOnlyList<Guid>> completedPeriods,
        CancellationToken cancellationToken)
    {
        foreach (var (userId, periodIds) in completedPeriods)
            await _badges.EvaluateAsync(userId, periodIds.Select(BadgeTriggerContext.ForGoalPeriod).ToList(), cancellationToken);
    }

    private async Task CloseDuePeriodsAsync(DateTime nowUtc, CancellationToken cancellationToken)
    {
        var periods = await _context.GoalPeriods.Include(period => period.Goal)
            .Where(period => period.Goal.Status == GoalStatus.Active && (period.Status == "Active" || period.Status == "Completed") && period.EndAt <= nowUtc)
            .ToListAsync(cancellationToken);
        foreach (var period in periods)
        {
            var failed = period.FailIfDue(nowUtc);
            if (failed)
            {
                if (!period.Goal.IsRecurring)
                    period.Goal.MarkFailed();
                await _notifications.CreateAsync(period.Goal.UserId, NotificationType.GoalPeriodFailed, "Your goal period ended before its target was reached.", referenceEntityId: period.GoalId, referenceEntityType: NotificationReferenceTypes.Goal, cancellationToken: cancellationToken);
            }
            if (!period.Goal.IsRecurring || string.IsNullOrWhiteSpace(period.Goal.RecurrencePattern)) continue;
            var (start, end) = GoalPeriodBoundaries.Next(period.EndAt, period.Goal.RecurrencePattern!, period.Goal.TimeZoneId);
            if (!await _context.GoalPeriods.AnyAsync(item => item.GoalId == period.GoalId && item.StartAt == start, cancellationToken))
            {
                await _context.GoalPeriods.AddAsync(new GoalPeriod(period.GoalId, start, end, period.Goal.TargetValue), cancellationToken);
                period.Goal.ResetCurrentPeriodProgress();
            }
        }
    }
}

public static class GoalPeriodBoundaries
{
    public static (DateTime Start, DateTime End) Current(string? recurrence, string timeZoneId, DateTime nowUtc)
    {
        if (string.IsNullOrWhiteSpace(recurrence)) return (nowUtc, nowUtc.AddYears(1));
        var zone = Resolve(timeZoneId); var localNow = TimeZoneInfo.ConvertTimeFromUtc(nowUtc, zone);
        var start = recurrence.ToLowerInvariant() switch
        {
            "daily" => localNow.Date,
            "weekly" => localNow.Date.AddDays(-((7 + (int)localNow.DayOfWeek - (int)DayOfWeek.Monday) % 7)),
            "monthly" => new DateTime(localNow.Year, localNow.Month, 1),
            _ => throw new ArgumentOutOfRangeException(nameof(recurrence))
        };
        var end = recurrence.ToLowerInvariant() switch { "daily" => start.AddDays(1), "weekly" => start.AddDays(7), "monthly" => start.AddMonths(1), _ => throw new ArgumentOutOfRangeException(nameof(recurrence)) };
        return (TimeZoneInfo.ConvertTimeToUtc(start, zone), TimeZoneInfo.ConvertTimeToUtc(end, zone));
    }
    public static (DateTime Start, DateTime End) Next(DateTime previousEndUtc, string recurrence, string timeZoneId)
    {
        var zone = Resolve(timeZoneId); var start = TimeZoneInfo.ConvertTimeFromUtc(previousEndUtc, zone).Date;
        var end = recurrence.ToLowerInvariant() switch { "daily" => start.AddDays(1), "weekly" => start.AddDays(7), "monthly" => start.AddMonths(1), _ => throw new ArgumentOutOfRangeException(nameof(recurrence)) };
        return (previousEndUtc, TimeZoneInfo.ConvertTimeToUtc(end, zone));
    }
    private static TimeZoneInfo Resolve(string id) { try { return TimeZoneInfo.FindSystemTimeZoneById(id); } catch { return TimeZoneInfo.Utc; } }
}
