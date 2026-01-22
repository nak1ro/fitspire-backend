using backend.Modules.Goal.Domain.Entities;
using backend.Modules.Shared.Domain;

namespace backend.Modules.Goal.Domain.Events;

public record GoalRecurringEvent(
    Guid OldGoalId,
    Guid UserId,
    Guid GoalTypeId,
    double TargetValue,
    string Unit,
    DateTime OldDeadline,
    string RecurrencePattern
) : IDomainEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
