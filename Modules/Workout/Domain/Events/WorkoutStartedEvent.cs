using backend.Modules.Shared.Domain;

namespace backend.Modules.Workout.Domain.Events;

public record WorkoutStartedEvent(Guid WorkoutId, Guid UserId, string WorkoutType) : IDomainEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
