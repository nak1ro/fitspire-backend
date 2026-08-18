using backend.Modules.AiCoaching.Domain;

namespace backend.Modules.AiCoaching.Services;

public static class CoachContextSnapshotVersions
{
    public const string Conversation = "coach-conversation-context-v2";
    public const string DailyBriefing = "coach-daily-briefing-context-v2";
}

public enum CoachSnapshotCoverageState
{
    Unavailable,
    Partial,
    Sufficient
}

public sealed record CoachConversationContextRequest(
    string Question,
    string? ThreadSummary,
    IReadOnlyList<CoachConversationHistoryMessage> RecentMessages,
    string TimeZoneId,
    DateTime RequestedAtUtc);

public sealed record CoachConversationHistoryMessage(string Role, string Content);

public sealed record CoachDailyBriefingContextRequest(string TimeZoneId, DateTime RequestedAtUtc);

public sealed record CoachContextSnapshotBuildResult(
    string SnapshotJson,
    string SourceFingerprint,
    IReadOnlySet<string> EvidenceKeys,
    IReadOnlyList<CoachIntent> Intents);

public sealed record CoachConversationContextSnapshot(
    string SchemaVersion,
    CoachSnapshotPeriod Period,
    string Question,
    string? ThreadSummary,
    IReadOnlyList<CoachConversationHistoryMessage> RecentMessages,
    IReadOnlyList<CoachIntent> Intents,
    CoachFitnessContextSnapshot Fitness,
    IReadOnlyList<CoachEvidence> Evidence);

public sealed record CoachDailyBriefingContextSnapshot(
    string SchemaVersion,
    CoachSnapshotPeriod Period,
    CoachFitnessContextSnapshot Fitness,
    IReadOnlyList<CoachEvidence> Evidence);

public sealed record CoachSnapshotPeriod(DateOnly StartDate, DateOnly EndDate, string TimeZoneId);

public sealed record CoachFitnessContextSnapshot(
    CoachWorkoutContextSnapshot? Workouts,
    IReadOnlyList<CoachGoalContextSnapshot>? Goals,
    IReadOnlyList<CoachChallengeContextSnapshot>? Challenges,
    CoachBodyContextSnapshot? Body,
    CoachNutritionContextSnapshot? Nutrition);

public sealed record CoachWorkoutContextSnapshot(
    CoachSectionCoverage Coverage,
    int WorkoutCount,
    int ActiveDays,
    double DurationMinutes,
    double CaloriesKcal,
    double DistanceKm,
    double GymVolumeKg,
    int PersonalRecordCount,
    IReadOnlyList<CoachWorkoutTypeCount> Types,
    IReadOnlyList<CoachWorkoutDaySnapshot> RecentDailyBreakdown,
    CoachWorkoutTotals? PreviousWindow);

public sealed record CoachWorkoutTotals(
    int WorkoutCount,
    int ActiveDays,
    double DurationMinutes,
    double CaloriesKcal,
    double DistanceKm,
    double GymVolumeKg);

public sealed record CoachWorkoutTypeCount(string WorkoutType, int Count);

public sealed record CoachWorkoutDaySnapshot(DateOnly Date, int WorkoutCount,
    IReadOnlyList<CoachWorkoutTypeSnapshot> Types);

public sealed record CoachWorkoutTypeSnapshot(string WorkoutType, int WorkoutCount, double DurationMinutes,
    double CaloriesKcal, double DistanceKm, double GymVolumeKg);

public sealed record CoachGoalContextSnapshot(
    string Label,
    string Unit,
    string Status,
    double TargetValue,
    double CurrentValue,
    int ProgressPercent,
    string DefinitionKey);

public sealed record CoachChallengeContextSnapshot(
    string Label,
    string MetricCode,
    string? WorkoutType,
    string Mode,
    string Status,
    double? TargetValue,
    double Score);

public sealed record CoachBodyContextSnapshot(
    CoachSectionCoverage Coverage,
    int CheckInCount,
    CoachMeasurementTrend WeightKg,
    CoachMeasurementTrend BodyFatPercent,
    CoachMeasurementTrend WaistCm,
    double? LatestWellbeingScore,
    double? AverageWellbeingScore);

public sealed record CoachMeasurementTrend(double? FirstValue, double? LastValue, double? Change);

public sealed record CoachNutritionContextSnapshot(
    CoachSectionCoverage Coverage,
    int LoggedDays,
    CoachNutritionTotals? AveragePerLoggedDay,
    CoachNutritionTargets? Targets,
    CoachNutritionPercentages? AverageTargetPercentages);

public sealed record CoachNutritionTotals(decimal CaloriesKcal, decimal ProteinGrams, decimal CarbsGrams,
    decimal FatGrams);

public sealed record CoachNutritionTargets(decimal? CaloriesKcal, decimal? ProteinGrams, decimal? CarbsGrams,
    decimal? FatGrams);

public sealed record CoachNutritionPercentages(decimal? CaloriesKcal, decimal? ProteinGrams, decimal? CarbsGrams,
    decimal? FatGrams);

public sealed record CoachSectionCoverage(CoachSnapshotCoverageState State, int RecordCount);

public sealed record CoachEvidence(string Key, string Description);
