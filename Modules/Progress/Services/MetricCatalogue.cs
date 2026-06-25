namespace backend.Modules.Progress.Services;

public static class MetricCatalogue
{
    public const string WorkoutCount = "workout.count";
    public const string DurationMinutes = "workout.duration.minutes";
    public const string LegacyCalories = "workout.calories";
    public const string LegacyDistanceKm = "workout.distance.km";
    public const string GymVolumeKg = "gym.volume.kg";
    public const string LegacyGymMaxWeightKg = "gym.max-weight.kg";

    public const string Calories = LegacyCalories;
    public const string DistanceKm = LegacyDistanceKm;
    public const string GymMaxWeightKg = LegacyGymMaxWeightKg;

    public const string CaloriesKcal = "workout.calories.kcal";
    public const string RunningDistanceKm = "running.distance.km";
    public const string CyclingDistanceKm = "cycling.distance.km";
    public const string SwimmingDistanceMeters = "swimming.distance.m";
    public const string YogaDurationMinutes = "yoga.duration.minutes";
    public const string GymExerciseCount = "gym.exercise_count";
    public const string ExerciseMaxWeightKg = "exercise.max-weight.kg";

    public static readonly IReadOnlyList<MetricDefinitionSeed> Definitions =
    [
        new(WorkoutCount, "Workouts", "count", "Sum", true, true, true, true),
        new(DurationMinutes, "Workout duration", "minutes", "Sum", true, true, true, true),
        new(CaloriesKcal, "Calories burned", "kcal", "Sum", true, true, true, true),
        new(RunningDistanceKm, "Running distance", "km", "Sum", true, true, true, true),
        new(CyclingDistanceKm, "Cycling distance", "km", "Sum", true, true, true, true),
        new(SwimmingDistanceMeters, "Swimming distance", "m", "Sum", true, true, true, true),
        new(YogaDurationMinutes, "Yoga duration", "minutes", "Sum", true, true, true, true),
        new(GymVolumeKg, "Gym volume", "kg", "Sum", true, true, true, true),
        new(GymExerciseCount, "Gym exercises", "count", "Sum", true, true, true, true),
        new(ExerciseMaxWeightKg, "Exercise max weight", "kg", "Maximum", true, false, true, true),
        new(LegacyCalories, "Legacy calories burned", "kcal", "Sum", true, false, false, true),
        new(LegacyDistanceKm, "Legacy distance", "km", "Sum", true, false, false, true),
        new(LegacyGymMaxWeightKg, "Legacy exercise max weight", "kg", "Maximum", true, false, false, true)
    ];
}

public sealed record MetricDefinitionSeed(
    string Code,
    string Name,
    string Unit,
    string Aggregation,
    bool IsGoalSupported,
    bool IsChallengeSupported,
    bool IsBadgeSupported,
    bool IsAnalyticsSupported);
