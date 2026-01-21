using backend.Modules.Shared.Domain;

namespace backend.Modules.Workout.Domain.Events;

public record WorkoutCompletedEvent(
    Guid WorkoutId, 
    Guid UserId, 
    string WorkoutType, 
    double? DurationMinutes
) : IDomainEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
