using backend.Modules.Goal.Domain.Enums;
using backend.Modules.Goal.Domain.Events;
using backend.Modules.Shared.Domain;
using backend.Modules.User.Domain;

namespace backend.Modules.Goal.Domain.Entities;

public class UserGoal : AggregateRoot<Guid>
{
    // Polish timezone for streak calculations
    private static readonly TimeZoneInfo PolishTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time");

    public Guid UserId { get; private set; }
    public Guid GoalTypeId { get; private set; }
    public double TargetValue { get; private set; }
    public double CurrentValue { get; private set; }
    public string Unit { get; private set; } = null!;
    public DateTime StartDate { get; private set; }
    public DateTime? Deadline { get; private set; }
    public bool IsRecurring { get; private set; }
    public string? RecurrencePattern { get; private set; } // "daily", "weekly", "monthly"
    public GoalStatus Status { get; private set; }
    public bool IsPublic { get; private set; }
    public int CurrentStreak { get; private set; }
    public DateTime? LastStreakDate { get; private set; }

    // Navigation
    public AppUser User { get; private set; } = null!;
    public GoalType GoalType { get; private set; } = null!;
    public ICollection<GoalProgressEntry> ProgressEntries { get; private set; } = new List<GoalProgressEntry>();

    private UserGoal() { }

    public UserGoal(
        Guid id,
        Guid userId,
        Guid goalTypeId,
        double targetValue,
        string unit,
        DateTime startDate,
        DateTime? deadline = null,
        bool isRecurring = false,
        string? recurrencePattern = null,
        bool isPublic = false)
    {
        Id = id;
        UserId = userId;
        GoalTypeId = goalTypeId;
        TargetValue = targetValue;
        Unit = unit;
        StartDate = startDate;
        Deadline = deadline;
        IsRecurring = isRecurring;
        RecurrencePattern = recurrencePattern;
        IsPublic = isPublic;
        Status = GoalStatus.Active;
        CurrentValue = 0;
        CurrentStreak = 0;
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates progress based on the goal's measurement type.
    /// </summary>
    public void UpdateProgress(double delta, GoalMeasurementType measurementType, DateTime eventDateUtc, TimeZoneInfo? timeZone = null)
    {
        if (Status != GoalStatus.Active) return;

        var previousValue = CurrentValue;

        switch (measurementType)
        {
            case GoalMeasurementType.Cumulative:
                CurrentValue += delta;
                break;

            case GoalMeasurementType.SingleEvent:
                // For single event, we check if the delta itself meets the target
                if (delta >= TargetValue)
                {
                    CurrentValue = delta;
                    MarkCompleted();
                }
                else if (delta > CurrentValue)
                {
                    CurrentValue = delta; // Track best attempt
                }
                break;

            case GoalMeasurementType.Threshold:
                // Threshold just updates to new value (snapshot)
                CurrentValue = delta;
                break;

            case GoalMeasurementType.Streak:
                UpdateStreak(eventDateUtc, timeZone ?? PolishTimeZone);
                break;
        }

        UpdatedAt = DateTime.UtcNow;

        // Check if goal is now complete (for Cumulative/Threshold)
        if (measurementType != GoalMeasurementType.SingleEvent && CurrentValue >= TargetValue)
        {
            MarkCompleted();
        }

        // Fire progress event
        AddDomainEvent(new GoalProgressUpdatedEvent(Id, UserId, previousValue, CurrentValue, GetMilestonePercent()));
    }

    /// <summary>
    /// Updates streak based on provided timezone.
    /// </summary>
    private void UpdateStreak(DateTime eventDateUtc, TimeZoneInfo localTimeZone)
    {
        var polishDate = TimeZoneInfo.ConvertTimeFromUtc(eventDateUtc, localTimeZone).Date;

        if (LastStreakDate == null)
        {
            // First streak entry
            CurrentStreak = 1;
            LastStreakDate = polishDate;
            CurrentValue = 1;
        }
        else
        {
            var lastPolishDate = LastStreakDate.Value;
            var daysDiff = (polishDate - lastPolishDate).Days;

            if (daysDiff == 0)
            {
                // Same day - no change
            }
            else if (daysDiff == 1)
            {
                // Consecutive day - increment streak
                CurrentStreak++;
                CurrentValue = CurrentStreak;
                LastStreakDate = polishDate;
            }
            else
            {
                // Streak broken - reset
                CurrentStreak = 1;
                CurrentValue = 1;
                LastStreakDate = polishDate;
            }
        }
    }

    /// <summary>
    /// Checks if streak has been broken (call daily).
    /// </summary>
    public void CheckStreakExpiry()
    {
        if (GoalType?.MeasurementType != GoalMeasurementType.Streak || Status != GoalStatus.Active)
            return;

        if (LastStreakDate == null) return;

        var polishNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, PolishTimeZone).Date;
        var daysSinceLastStreak = (polishNow - LastStreakDate.Value).Days;

        if (daysSinceLastStreak > 1)
        {
            // Streak broken
            CurrentStreak = 0;
            CurrentValue = 0;
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public void MarkCompleted()
    {
        if (Status == GoalStatus.Completed) return;

        Status = GoalStatus.Completed;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new GoalCompletedEvent(Id, UserId, GoalTypeId, TargetValue, CurrentValue));

        if (IsRecurring && !string.IsNullOrEmpty(RecurrencePattern) && Deadline.HasValue)
        {
            AddDomainEvent(new GoalRecurringEvent(Id, UserId, GoalTypeId, TargetValue, Unit, Deadline.Value, RecurrencePattern));
        }
    }

    public void MarkFailed()
    {
        Status = GoalStatus.Failed;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Archive()
    {
        Status = GoalStatus.Archived;
        UpdatedAt = DateTime.UtcNow;
    }

    public int GetMilestonePercent()
    {
        if (TargetValue <= 0) return 0;
        var percent = (CurrentValue / TargetValue) * 100;
        if (percent >= 100) return 100;
        if (percent >= 75) return 75;
        if (percent >= 50) return 50;
        if (percent >= 25) return 25;
        return 0;
    }

    public bool IsExpired()
    {
        return Deadline.HasValue && DateTime.UtcNow > Deadline.Value && Status == GoalStatus.Active;
    }
}
