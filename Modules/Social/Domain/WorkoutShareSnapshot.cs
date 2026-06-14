namespace backend.Modules.Social.Domain;

public class WorkoutShareSnapshot
{
    public Guid SourceWorkoutId { get; private set; }
    public string WorkoutType { get; private set; } = null!;
    public DateTime WorkoutDate { get; private set; }
    public double? DurationMinutes { get; private set; }
    public double? DistanceKm { get; private set; }
    public int? CaloriesBurned { get; private set; }
    public double? TotalVolumeKg { get; private set; }
    public int? ExerciseCount { get; private set; }
    public DateTime? CompletedAt { get; private set; }

    private WorkoutShareSnapshot() { }

    public WorkoutShareSnapshot(
        Guid sourceWorkoutId,
        string workoutType,
        DateTime workoutDate,
        double? durationMinutes,
        double? distanceKm,
        int? caloriesBurned,
        double? totalVolumeKg,
        int? exerciseCount,
        DateTime? completedAt)
    {
        if (sourceWorkoutId == Guid.Empty)
            throw new ArgumentException("Source workout id is required.", nameof(sourceWorkoutId));
        if (string.IsNullOrWhiteSpace(workoutType))
            throw new ArgumentException("Workout type is required.", nameof(workoutType));

        SourceWorkoutId = sourceWorkoutId;
        WorkoutType = workoutType.Trim();
        WorkoutDate = workoutDate;
        DurationMinutes = durationMinutes;
        DistanceKm = distanceKm;
        CaloriesBurned = caloriesBurned;
        TotalVolumeKg = totalVolumeKg;
        ExerciseCount = exerciseCount;
        CompletedAt = completedAt;
    }
}
