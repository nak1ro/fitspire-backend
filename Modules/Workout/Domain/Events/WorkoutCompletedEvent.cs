using backend.Modules.Shared.Domain;

namespace backend.Modules.Workout.Domain.Events;

public record WorkoutCompletedEvent(
    Guid WorkoutId,
    Guid UserId,
    string WorkoutType,
    bool IsPrivate,
    double? DurationMinutes,
    double? DistanceKm,
    int? CaloriesBurned,
    double? TotalVolumeKg,
    string? Notes
) : IDomainEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
