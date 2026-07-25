using System.Text.Json.Serialization;

namespace backend.Modules.AiCoaching.Services;

public static class WeeklyCoachSnapshotVersions
{
    public const string Snapshot = "weekly-coach-snapshot-v1";
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum WeeklyCoachCoverage
{
    Unavailable,
    Partial,
    Sufficient
}

public sealed record WeeklyCoachSnapshot(
    string SchemaVersion,
    WeeklyCoachSnapshotPeriod Period,
    WeeklyCoachSnapshotCoverage Coverage,
    WeeklyCoachWorkoutSnapshot Workouts,
    IReadOnlyList<WeeklyCoachGoalSnapshot> Goals,
    IReadOnlyList<WeeklyCoachChallengeSnapshot> Challenges,
    WeeklyCoachBodySnapshot Body,
    WeeklyCoachNutritionSnapshot Nutrition,
    IReadOnlyList<WeeklyCoachEvidence> Evidence);

public sealed record WeeklyCoachSnapshotPeriod(DateOnly Start, DateOnly End, string TimeZoneId);

public sealed record WeeklyCoachSnapshotCoverage(
    WeeklyCoachSectionCoverage Workouts,
    WeeklyCoachSectionCoverage Goals,
    WeeklyCoachSectionCoverage Challenges,
    WeeklyCoachSectionCoverage Body,
    WeeklyCoachSectionCoverage Nutrition);

public sealed record WeeklyCoachSectionCoverage(WeeklyCoachCoverage State, int RecordCount);

public sealed record WeeklyCoachWorkoutSnapshot(
    int WorkoutCount,
    int ActiveDays,
    double DurationMinutes,
    double CaloriesKcal,
    double DistanceKm,
    double GymVolumeKg,
    int PersonalRecordCount,
    IReadOnlyList<WeeklyCoachWorkoutTypeCount> Types,
    WeeklyCoachWorkoutTotals PreviousWeek);

public sealed record WeeklyCoachWorkoutTotals(
    int WorkoutCount,
    int ActiveDays,
    double DurationMinutes,
    double CaloriesKcal,
    double DistanceKm,
    double GymVolumeKg);

public sealed record WeeklyCoachWorkoutTypeCount(string WorkoutType, int Count);

public sealed record WeeklyCoachGoalSnapshot(
    string Name,
    string Unit,
    string Status,
    double TargetValue,
    double? ProgressValueAtPeriodEnd,
    double ProgressDeltaDuringPeriod,
    bool HasProgressRecord);

public sealed record WeeklyCoachChallengeSnapshot(
    string Label,
    string MetricCode,
    string? WorkoutType,
    string Mode,
    string Status,
    double? TargetValue,
    double ProgressDuringPeriod,
    bool? Finished,
    int? Rank);

public sealed record WeeklyCoachBodySnapshot(
    int CheckInCount,
    WeeklyCoachMeasurementTrend WeightKg,
    WeeklyCoachMeasurementTrend BodyFatPercent,
    WeeklyCoachMeasurementTrend WaistCm,
    double? AverageWellbeingScore,
    double? PreviousWeekAverageWellbeingScore);

public sealed record WeeklyCoachMeasurementTrend(double? FirstValue, double? LastValue, double? Change);

public sealed record WeeklyCoachNutritionSnapshot(
    int LoggedDayCount,
    WeeklyCoachNutritionTotals? AveragePerLoggedDay,
    WeeklyCoachNutritionTotals? PreviousWeekAveragePerLoggedDay,
    WeeklyCoachNutritionTargets? Targets,
    WeeklyCoachNutritionPercentages? AverageTargetPercentages);

public sealed record WeeklyCoachNutritionTotals(decimal CaloriesKcal, decimal ProteinGrams, decimal CarbsGrams,
    decimal FatGrams);

public sealed record WeeklyCoachNutritionTargets(decimal? CaloriesKcal, decimal? ProteinGrams, decimal? CarbsGrams,
    decimal? FatGrams);

public sealed record WeeklyCoachNutritionPercentages(decimal? CaloriesKcal, decimal? ProteinGrams, decimal? CarbsGrams,
    decimal? FatGrams);

public sealed record WeeklyCoachEvidence(string Key, string Description);

public sealed record WeeklyCoachSnapshotBuildResult(
    WeeklyCoachSnapshot Snapshot,
    string SnapshotJson,
    string SourceFingerprint,
    IReadOnlySet<string> EvidenceKeys);
