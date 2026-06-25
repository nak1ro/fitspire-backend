using backend.Data;
using backend.Modules.Goal.Domain.Entities;
using backend.Modules.Goal.Domain.Enums;
using backend.Modules.Notification.Domain.Constants;
using backend.Modules.Notification.Domain.Enums;
using backend.Modules.Notification.Services;
using Microsoft.EntityFrameworkCore;

namespace backend.Modules.Goal.Services;

public interface IGoalProgressService
{
    Task RecalculateForUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task ProcessDuePeriodsAsync(DateTime nowUtc, CancellationToken cancellationToken = default);
}

public class GoalProgressService : IGoalProgressService
{
    private readonly FitspireDbContext _context;
    private readonly INotificationService _notifications;
    public GoalProgressService(FitspireDbContext context, INotificationService notifications) { _context = context; _notifications = notifications; }

    public async Task RecalculateForUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var periods = await _context.GoalPeriods.Include(period => period.Goal).ThenInclude(goal => goal.GoalType)
            .Where(period => period.Goal.UserId == userId && (period.Status == "Active" || period.Status == "Completed"))
            .ToListAsync(cancellationToken);
        foreach (var period in periods)
        {
            var goal = period.Goal;
            if (string.IsNullOrWhiteSpace(goal.GoalType.MetricCode))
                continue;
            var query = _context.ActivityContributions.Where(contribution => contribution.UserId == userId && contribution.IsActive &&
                contribution.MetricCode == goal.GoalType.MetricCode && contribution.OccurredAt >= period.StartAt && contribution.OccurredAt < period.EndAt);
            if (!string.IsNullOrWhiteSpace(goal.SelectedWorkoutType)) query = query.Where(contribution => contribution.WorkoutType == goal.SelectedWorkoutType);
            if (goal.SelectedExerciseId.HasValue) query = query.Where(contribution => contribution.ExerciseId == goal.SelectedExerciseId);
            var values = await query.Select(contribution => contribution.Value).ToListAsync(cancellationToken);
            var progress = goal.GoalType.MeasurementType == GoalMeasurementType.SingleEvent ? values.DefaultIfEmpty(0).Max() : values.Sum();
            var wasCompleted = period.Status == "Completed";
            period.SetProgress(progress);
            goal.RestoreProgress(progress);
            if (!wasCompleted && period.Status == "Completed")
                await _notifications.CreateAsync(userId, NotificationType.GoalCompleted, $"You completed your goal: {goal.GoalType.Name}.", referenceEntityId: goal.Id, referenceEntityType: NotificationReferenceTypes.Goal, cancellationToken: cancellationToken);
        }
    }

    public async Task ProcessDuePeriodsAsync(DateTime nowUtc, CancellationToken cancellationToken = default)
    {
        var periods = await _context.GoalPeriods.Include(period => period.Goal)
            .Where(period => period.Status == "Active" && period.EndAt <= nowUtc).ToListAsync(cancellationToken);
        foreach (var period in periods)
        {
            if (!period.FailIfDue(nowUtc)) continue;
            await _notifications.CreateAsync(period.Goal.UserId, NotificationType.GoalPeriodFailed, "Your goal period ended before its target was reached.", referenceEntityId: period.GoalId, referenceEntityType: NotificationReferenceTypes.Goal, cancellationToken: cancellationToken);
            if (!period.Goal.IsRecurring || string.IsNullOrWhiteSpace(period.Goal.RecurrencePattern)) continue;
            var (start, end) = GoalPeriodBoundaries.Next(period.EndAt, period.Goal.RecurrencePattern!, period.Goal.TimeZoneId);
            if (!await _context.GoalPeriods.AnyAsync(item => item.GoalId == period.GoalId && item.StartAt == start, cancellationToken))
                await _context.GoalPeriods.AddAsync(new GoalPeriod(period.GoalId, start, end, period.Goal.TargetValue), cancellationToken);
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
