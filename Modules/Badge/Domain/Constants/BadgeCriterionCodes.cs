namespace backend.Modules.Badge.Domain.Constants;

public static class BadgeCriterionCodes
{
    public const string WorkoutCount = "workout.count";
    public const string WorkoutLongestStreakDays = "workout.streak.longest_days";
    public const string RunningDistanceTotalKm = "running.distance.total_km";
    public const string CyclingDistanceTotalKm = "cycling.distance.total_km";
    public const string SwimmingDistanceTotalMeters = "swimming.distance.total_m";
    public const string WorkoutDurationTotalMinutes = "workout.duration.total_minutes";
    public const string GymVolumeTotalKg = "gym.volume.total_kg";
    public const string PersonalRecordAchievementCount = "personal_record.achievement_count";
    public const string GoalPeriodCompletionCount = "goal.period_completion_count";
    public const string ChallengeParticipationCount = "challenge.participation_count";
    public const string ChallengeTargetCompletionCount = "challenge.target_completion_count";
    public const string ChallengeWinCount = "challenge.win_count";
    public const string WorkoutShareCount = "social.workout_share_count";

    private static readonly HashSet<string> Known =
    [
        WorkoutCount,
        WorkoutLongestStreakDays,
        RunningDistanceTotalKm,
        CyclingDistanceTotalKm,
        SwimmingDistanceTotalMeters,
        WorkoutDurationTotalMinutes,
        GymVolumeTotalKg,
        PersonalRecordAchievementCount,
        GoalPeriodCompletionCount,
        ChallengeParticipationCount,
        ChallengeTargetCompletionCount,
        ChallengeWinCount,
        WorkoutShareCount
    ];

    public static bool IsKnown(string? value) => value is not null && Known.Contains(value);
}
