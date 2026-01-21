using backend.Modules.Shared.Domain;

namespace backend.Modules.Workout.Domain.Events;

public record PersonalRecordBrokenEvent(
    Guid RecordId,
    Guid UserId,
    string WorkoutType,
    string Metric,
    double PreviousValue,
    double NewValue
) : IDomainEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
