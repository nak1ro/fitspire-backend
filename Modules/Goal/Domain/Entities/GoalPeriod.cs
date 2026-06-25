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
        if (endAt <= startAt)
            throw new DomainException("A goal period must end after it starts.");

        Id = Guid.NewGuid();
        GoalId = goalId;
        StartAt = startAt;
        EndAt = endAt;
        TargetValue = targetValue;
        Status = "Active";
        CreatedAt = DateTime.UtcNow;
    }

    public void SetProgress(double value)
    {
        ProgressValue = Math.Max(0, value);
        if (Status == "Active" && ProgressValue >= TargetValue)
        {
            Status = "Completed";
            CompletedAt = DateTime.UtcNow;
        }
        else if (Status == "Completed" && ProgressValue < TargetValue)
        {
            Status = "Active";
            CompletedAt = null;
        }

        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateTarget(double targetValue)
    {
        if (targetValue <= 0)
            throw new DomainException("Goal target must be greater than zero.");

        TargetValue = targetValue;
        SetProgress(ProgressValue);
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
