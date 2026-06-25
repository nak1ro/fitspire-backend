using backend.Modules.Shared.Domain;

namespace backend.Modules.Goal.Domain.Entities;

public class GoalTargetChange : Entity<Guid>
{
    public Guid GoalId { get; private set; }
    public double PreviousTargetValue { get; private set; }
    public double NewTargetValue { get; private set; }
    public DateTime ChangedAt { get; private set; }

    public UserGoal Goal { get; private set; } = null!;

    private GoalTargetChange() { }

    public GoalTargetChange(Guid goalId, double previousTargetValue, double newTargetValue)
    {
        if (goalId == Guid.Empty)
            throw new DomainException("Goal target history requires a goal.");

        Id = Guid.NewGuid();
        GoalId = goalId;
        PreviousTargetValue = previousTargetValue;
        NewTargetValue = newTargetValue;
        ChangedAt = DateTime.UtcNow;
        CreatedAt = ChangedAt;
    }
}
