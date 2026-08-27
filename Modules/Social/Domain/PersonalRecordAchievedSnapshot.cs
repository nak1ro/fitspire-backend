namespace backend.Modules.Social.Domain;

public class PersonalRecordAchievedSnapshot
{
    public Guid SourcePersonalRecordId { get; private set; }
    public string WorkoutType { get; private set; } = null!;
    public string Metric { get; private set; } = null!;
    public Guid? ExerciseId { get; private set; }
    public string? ExerciseName { get; private set; }
    public double Value { get; private set; }
    public string Unit { get; private set; } = null!;
    public DateTime AchievedAt { get; private set; }

    private PersonalRecordAchievedSnapshot() { }

    public PersonalRecordAchievedSnapshot(
        Guid sourcePersonalRecordId,
        string workoutType,
        string metric,
        Guid? exerciseId,
        string? exerciseName,
        double value,
        string unit,
        DateTime achievedAt)
    {
        if (sourcePersonalRecordId == Guid.Empty)
            throw new ArgumentException("Source personal record id is required.", nameof(sourcePersonalRecordId));
        if (string.IsNullOrWhiteSpace(workoutType))
            throw new ArgumentException("Workout type is required.", nameof(workoutType));
        if (string.IsNullOrWhiteSpace(metric))
            throw new ArgumentException("Metric is required.", nameof(metric));
        if (string.IsNullOrWhiteSpace(unit))
            throw new ArgumentException("Unit is required.", nameof(unit));

        SourcePersonalRecordId = sourcePersonalRecordId;
        WorkoutType = workoutType.Trim();
        Metric = metric.Trim();
        ExerciseId = exerciseId;
        ExerciseName = exerciseName?.Trim();
        Value = value;
        Unit = unit.Trim();
        AchievedAt = achievedAt;
    }
}
