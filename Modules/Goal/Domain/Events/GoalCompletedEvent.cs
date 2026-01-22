using backend.Modules.Shared.Domain;

namespace backend.Modules.Goal.Domain.Events;

public record GoalCompletedEvent(
    Guid GoalId,
    Guid UserId,
    Guid GoalTypeId,
    double TargetValue,
    double FinalValue
) : IDomainEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
