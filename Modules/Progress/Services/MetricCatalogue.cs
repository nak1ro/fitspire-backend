namespace backend.Modules.Progress.Services;

public static class MetricCatalogue
{
    public const string WorkoutCount = "workout.count";
    public const string DurationMinutes = "workout.duration.minutes";
    public const string Calories = "workout.calories";
    public const string DistanceKm = "workout.distance.km";
    public const string GymVolumeKg = "gym.volume.kg";
    public const string GymMaxWeightKg = "gym.max-weight.kg";

    public static readonly IReadOnlyList<(string Code, string Name, string Unit, string Aggregation)> Definitions =
    [
        (WorkoutCount, "Workouts", "count", "Sum"),
        (DurationMinutes, "Workout duration", "minutes", "Sum"),
        (Calories, "Calories burned", "kcal", "Sum"),
        (DistanceKm, "Distance", "km", "Sum"),
        (GymVolumeKg, "Gym volume", "kg", "Sum"),
        (GymMaxWeightKg, "Exercise max weight", "kg", "Maximum")
    ];
}
