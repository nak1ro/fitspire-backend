using backend.Modules.Shared.Domain;

namespace backend.Modules.Progress.Domain;

public class ActivityContribution : Entity<Guid>
{
    public Guid UserId { get; private set; }
    public Guid SourceWorkoutId { get; private set; }
    public string MetricCode { get; private set; } = null!;
    public double Value { get; private set; }
    public string WorkoutType { get; private set; } = null!;
    public Guid? ExerciseId { get; private set; }
    public DateTime OccurredAt { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime? DeactivatedAt { get; private set; }

    private ActivityContribution() { }

    public ActivityContribution(Guid userId, Guid sourceWorkoutId, string metricCode, double value, string workoutType, DateTime occurredAt, Guid? exerciseId = null)
    {
        if (value < 0)
            throw new DomainException("A contribution value cannot be negative.");

        Id = Guid.NewGuid();
        UserId = userId;
        SourceWorkoutId = sourceWorkoutId;
        MetricCode = metricCode;
        Value = value;
        WorkoutType = workoutType;
        OccurredAt = occurredAt;
        ExerciseId = exerciseId;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        if (!IsActive)
            return;

        IsActive = false;
        DeactivatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Replace(double value, string workoutType, DateTime occurredAt)
    {
        if (value < 0)
            throw new DomainException("A contribution value cannot be negative.");

        Value = value;
        WorkoutType = workoutType;
        OccurredAt = occurredAt;
        IsActive = true;
        DeactivatedAt = null;
        UpdatedAt = DateTime.UtcNow;
    }
}
