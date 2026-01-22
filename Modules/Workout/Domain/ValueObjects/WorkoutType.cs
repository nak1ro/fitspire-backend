using backend.Modules.Shared.Domain;

namespace backend.Modules.Workout.Domain.ValueObjects;

/// <summary>
/// Value object representing the type of workout.
/// </summary>
public sealed class WorkoutType : ValueObject
{
    public static readonly WorkoutType Gym = new("gym");
    public static readonly WorkoutType Cycling = new("cycling");
    public static readonly WorkoutType Swimming = new("swimming");
    public static readonly WorkoutType Yoga = new("yoga");
    public static readonly WorkoutType Running = new("running");

    public string Value { get; }

    // Private constructor for known types
    private WorkoutType(string value) => Value = value;

    // Factory method with validation
    public static WorkoutType FromString(string value) => value.ToLowerInvariant() switch
    {
        "gym" => Gym,
        "cycling" => Cycling,
        "swimming" => Swimming,
        "yoga" => Yoga,
        "running" => Running,
        _ => throw new DomainException($"Invalid workout type: {value}")
    };

    public override string ToString() => Value;

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
