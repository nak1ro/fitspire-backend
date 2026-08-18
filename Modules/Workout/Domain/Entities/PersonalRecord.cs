using backend.Modules.Shared.Domain;
using backend.Modules.User.Domain;
using backend.Modules.Workout.Domain.Events;

namespace backend.Modules.Workout.Domain.Entities;

public class PersonalRecord : AggregateRoot<Guid>
{
    public Guid UserId { get; private set; }
    public string WorkoutType { get; private set; } = null!;
    public string Metric { get; private set; } = null!;
    public Guid? ExerciseId { get; private set; }
    public double Value { get; private set; }
    public Guid WorkoutId { get; private set; }
    public bool IsFeatured { get; private set; }

    /// <summary>
    /// The business date the record was set on — the occurrence date of the workout that
    /// earned it, not the wall-clock time the recalculation happened to run.
    /// </summary>
    public DateTime AchievedAt { get; private set; }

    // Navigation
    public AppUser User { get; private set; } = null!;
    public UserWorkout UserWorkout { get; private set; } = null!;
    public Exercise? Exercise { get; private set; }

    // EF Core constructor
    private PersonalRecord() { }

    private PersonalRecord(Guid id, Guid userId, string workoutType, string metric, Guid? exerciseId, double value, Guid workoutId, DateTime achievedAt)
    {
        Id = id;
        UserId = userId;
        WorkoutType = workoutType;
        Metric = metric;
        ExerciseId = exerciseId;
        Value = value;
        WorkoutId = workoutId;
        AchievedAt = achievedAt;
        CreatedAt = DateTime.UtcNow;
    }

    public static PersonalRecord Create(Guid userId, string workoutType, string metric, Guid? exerciseId, double value, Guid workoutId, DateTime achievedAt)
    {
        if (value <= 0)
            throw new DomainException("Personal record value must be positive.");

        return new PersonalRecord(Guid.NewGuid(), userId, workoutType, metric, exerciseId, value, workoutId, achievedAt);
    }

    public bool TryBeat(double newValue, Guid workoutId, DateTime achievedAt)
    {
        if (newValue <= Value)
            return false;

        var previousValue = Value;
        Value = newValue;
        WorkoutId = workoutId;
        AchievedAt = achievedAt;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new PersonalRecordBrokenEvent(Id, UserId, WorkoutType, Metric, previousValue, newValue));

        return true;
    }

    public void Replace(double value, Guid workoutId, DateTime achievedAt)
    {
        if (value <= 0)
            throw new DomainException("Personal record value must be positive.");

        Value = value;
        WorkoutId = workoutId;
        AchievedAt = achievedAt;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetFeatured()
    {
        IsFeatured = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ClearFeatured()
    {
        IsFeatured = false;
        UpdatedAt = DateTime.UtcNow;
    }
}
