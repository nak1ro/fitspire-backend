namespace backend.Modules.Workout.Services;

public static class PersonalRecordMetricCatalogue
{
    public const string DurationMinutes = "duration";
    public const string Calories = "calories";
    public const string Distance = "distance";
    public const string TotalVolume = "total_volume";
    public const string MaximumWeight = "max_weight";
    public const string MaximumSetVolume = "max_set_volume";
    public const string MaximumReps = "max_reps";
    public const string EstimatedOneRepMax = "estimated_1rm";

    public static readonly IReadOnlyDictionary<string, string> Units = new Dictionary<string, string>
    {
        [DurationMinutes] = "minutes", [Calories] = "kcal", [Distance] = "km",
        [TotalVolume] = "kg", [MaximumWeight] = "kg", [MaximumSetVolume] = "kg",
        [MaximumReps] = "reps", [EstimatedOneRepMax] = "kg"
    };
}
