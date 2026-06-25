using backend.Modules.Goal.Domain.Constants;
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
    public string? RecurrencePattern { get; private set; }
    public GoalStatus Status { get; private set; }
    public bool IsPublic { get; private set; }
    public int CurrentStreak { get; private set; }
    public DateTime? LastStreakDate { get; private set; }
    public string TimeZoneId { get; private set; } = "Central European Standard Time";
    public string? SelectedWorkoutType { get; private set; }
    public Guid? SelectedExerciseId { get; private set; }
    public string DefinitionKey { get; private set; } = null!;

    public AppUser User { get; private set; } = null!;
    public GoalType GoalType { get; private set; } = null!;
    public ICollection<GoalProgressEntry> ProgressEntries { get; private set; } = new List<GoalProgressEntry>();
    public ICollection<GoalPeriod> Periods { get; private set; } = new List<GoalPeriod>();

    private UserGoal() { }

    public UserGoal(Guid id, Guid userId, Guid goalTypeId, double targetValue, string unit, DateTime startDate,
        DateTime? deadline, bool isRecurring, string? recurrencePattern, bool isPublic)
    {
        ValidateCreation(userId, goalTypeId, targetValue, unit, deadline, isRecurring, recurrencePattern);
        Id = id;
        UserId = userId;
        GoalTypeId = goalTypeId;
        TargetValue = targetValue;
        Unit = unit.Trim();
        StartDate = startDate.ToUniversalTime();
        Deadline = deadline?.ToUniversalTime();
        IsRecurring = isRecurring;
        RecurrencePattern = recurrencePattern?.Trim().ToLowerInvariant();
        IsPublic = isPublic;
        Status = GoalStatus.Active;
        CreatedAt = DateTime.UtcNow;
    }

    public bool ApplyCurrentPeriodProgress(double currentValue, bool periodCompleted)
    {
        if (Status != GoalStatus.Active)
            return false;

        var normalizedValue = Math.Max(0, currentValue);
        var changed = CurrentValue != normalizedValue;
        CurrentValue = normalizedValue;
        UpdatedAt = DateTime.UtcNow;
        if (!IsRecurring && periodCompleted)
            MarkCompleted();
        return changed;
    }

    public void ResetCurrentPeriodProgress()
    {
        if (Status != GoalStatus.Active || !IsRecurring)
            return;

        CurrentValue = 0;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetTemplateParameters(string timeZoneId, string? selectedWorkoutType, Guid? selectedExerciseId)
    {
        if (string.IsNullOrWhiteSpace(timeZoneId))
            throw new DomainException("Goal timezone is required.");

        TimeZoneId = timeZoneId.Trim();
        SelectedWorkoutType = selectedWorkoutType?.Trim().ToLowerInvariant();
        SelectedExerciseId = selectedExerciseId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetDefinitionKey(string definitionKey)
    {
        if (string.IsNullOrWhiteSpace(definitionKey))
            throw new DomainException("Goal definition is required.");

        DefinitionKey = definitionKey;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateTarget(double targetValue, bool isPublic)
    {
        if (Status != GoalStatus.Active)
            throw new DomainException("Only active goals can be edited.");
        if (targetValue <= 0)
            throw new DomainException("Goal target must be greater than zero.");

        TargetValue = targetValue;
        IsPublic = isPublic;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkFailed()
    {
        if (Status != GoalStatus.Active)
            return;

        Status = GoalStatus.Failed;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Archive()
    {
        if (Status == GoalStatus.Archived)
            return;
        if (Status != GoalStatus.Active)
            throw new DomainException("Only active goals can be archived.");

        Status = GoalStatus.Archived;
        UpdatedAt = DateTime.UtcNow;
    }

    public int GetMilestonePercent()
    {
        if (TargetValue <= 0)
            return 0;
        var percent = CurrentValue / TargetValue * 100;
        return percent switch
        {
            >= 100 => 100,
            >= 75 => 75,
            >= 50 => 50,
            >= 25 => 25,
            _ => 0
        };
    }

    private void MarkCompleted()
    {
        Status = GoalStatus.Completed;
        UpdatedAt = DateTime.UtcNow;
    }

    private static void ValidateCreation(Guid userId, Guid goalTypeId, double targetValue, string unit, DateTime? deadline,
        bool isRecurring, string? recurrencePattern)
    {
        if (userId == Guid.Empty || goalTypeId == Guid.Empty)
            throw new DomainException("Goal owner and template are required.");
        if (targetValue <= 0 || double.IsNaN(targetValue) || double.IsInfinity(targetValue))
            throw new DomainException("Goal target must be greater than zero.");
        if (string.IsNullOrWhiteSpace(unit))
            throw new DomainException("Goal unit is required.");
        if (isRecurring && (!string.IsNullOrWhiteSpace(recurrencePattern) && GoalSchedules.Recurring.Contains(recurrencePattern)))
        {
            if (deadline.HasValue)
                throw new DomainException("Recurring goals do not use an overall deadline.");
            return;
        }
        if (isRecurring)
            throw new DomainException("Recurring goals require a supported recurrence pattern.");
        if (deadline is null || deadline.Value <= DateTime.UtcNow)
            throw new DomainException("One-off goals require a future deadline.");
        if (!string.IsNullOrWhiteSpace(recurrencePattern))
            throw new DomainException("One-off goals do not use a recurrence pattern.");
    }
}
