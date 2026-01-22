using backend.Modules.Goal.Domain.Enums;
using backend.Modules.Shared.Domain;
using backend.Modules.User.Domain;

namespace backend.Modules.Goal.Domain.Entities;

public class UserGoal : AggregateRoot<Guid>
{
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

    // Placeholder methods - to be implemented with rich logic later
    public void UpdateProgress(double delta)
    {
        CurrentValue += delta;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkCompleted()
    {
        Status = GoalStatus.Completed;
        UpdatedAt = DateTime.UtcNow;
    }

    public int GetMilestonePercent()
    {
        var percent = (CurrentValue / TargetValue) * 100;
        if (percent >= 100) return 100;
        if (percent >= 75) return 75;
        if (percent >= 50) return 50;
        if (percent >= 25) return 25;
        return 0;
    }
}
