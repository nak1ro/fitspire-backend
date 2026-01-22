using backend.Modules.Shared.Domain;

namespace backend.Modules.Goal.Domain.Events;

public record GoalProgressUpdatedEvent(
    Guid GoalId,
    Guid UserId,
    double PreviousValue,
    double NewValue,
    int MilestonePercent
) : IDomainEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
