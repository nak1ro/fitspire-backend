using backend.Modules.Shared.Domain;
using backend.Modules.User.Domain;
using backend.Modules.Workout.Domain.Events;

namespace backend.Modules.Workout.Domain.Entities;

public class PersonalRecord : AggregateRoot<Guid>
{
    public Guid UserId { get; private set; }
    public string WorkoutType { get; private set; } = null!;
    public string Metric { get; private set; } = null!;
    public double Value { get; private set; }
    public Guid WorkoutId { get; private set; }

    // Navigation
    public AppUser User { get; private set; } = null!;
    public UserWorkout UserWorkout { get; private set; } = null!;

    // EF Core constructor
    private PersonalRecord() { }

    private PersonalRecord(Guid id, Guid userId, string workoutType, string metric, double value, Guid workoutId)
    {
        Id = id;
        UserId = userId;
        WorkoutType = workoutType;
        Metric = metric;
        Value = value;
        WorkoutId = workoutId;
        CreatedAt = DateTime.UtcNow;
    }

    public static PersonalRecord Create(Guid userId, string workoutType, string metric, double value, Guid workoutId)
    {
        if (value <= 0)
            throw new DomainException("Personal record value must be positive.");

        return new PersonalRecord(Guid.NewGuid(), userId, workoutType, metric, value, workoutId);
    }

    public bool TryBeat(double newValue, Guid workoutId)
    {
        if (newValue <= Value)
            return false;

        var previousValue = Value;
        Value = newValue;
        WorkoutId = workoutId;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new PersonalRecordBrokenEvent(Id, UserId, WorkoutType, Metric, previousValue, newValue));

        return true;
    }
}