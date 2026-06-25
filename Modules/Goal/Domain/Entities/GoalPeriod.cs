using backend.Modules.Shared.Domain;

namespace backend.Modules.Goal.Domain.Entities;

public class GoalPeriod : Entity<Guid>
{
    public Guid GoalId { get; private set; }
    public DateTime StartAt { get; private set; }
    public DateTime EndAt { get; private set; }
    public double TargetValue { get; private set; }
    public double ProgressValue { get; private set; }
    public string Status { get; private set; } = "Active";
    public DateTime? CompletedAt { get; private set; }
    public DateTime? FailedAt { get; private set; }

    public UserGoal Goal { get; private set; } = null!;

    private GoalPeriod() { }

    public GoalPeriod(Guid goalId, DateTime startAt, DateTime endAt, double targetValue)
    {
        if (goalId == Guid.Empty)
            throw new DomainException("Goal period requires a goal.");
        if (endAt <= startAt)
            throw new DomainException("A goal period must end after it starts.");
        if (targetValue <= 0 || double.IsNaN(targetValue) || double.IsInfinity(targetValue))
            throw new DomainException("Goal target must be greater than zero.");

        Id = Guid.NewGuid();
        GoalId = goalId;
        StartAt = startAt;
        EndAt = endAt;
        TargetValue = targetValue;
        Status = "Active";
        CreatedAt = DateTime.UtcNow;
    }

    public bool SetProgress(double value)
    {
        if (Status != "Active")
            return false;

        var previousValue = ProgressValue;
        ProgressValue = Math.Max(0, value);
        if (ProgressValue >= TargetValue)
        {
            Status = "Completed";
            CompletedAt = DateTime.UtcNow;
        }

        UpdatedAt = DateTime.UtcNow;
        return previousValue != ProgressValue;
    }

    public void UpdateTarget(double targetValue)
    {
        if (Status != "Active")
            throw new DomainException("Only active goal periods can be edited.");
        if (targetValue <= 0)
            throw new DomainException("Goal target must be greater than zero.");

        TargetValue = targetValue;
        SetProgress(ProgressValue);
    }

    public void UpdateEndAt(DateTime endAt)
    {
        if (Status != "Active")
            throw new DomainException("Only active goal periods can be edited.");
        if (endAt <= StartAt)
            throw new DomainException("A goal deadline must be after the period start.");

        EndAt = endAt.ToUniversalTime();
        UpdatedAt = DateTime.UtcNow;
    }

    public bool FailIfDue(DateTime nowUtc)
    {
        if (Status != "Active" || EndAt > nowUtc)
            return false;

        Status = "Failed";
        FailedAt = nowUtc;
        UpdatedAt = nowUtc;
        return true;
    }
}
