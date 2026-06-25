using backend.Modules.Badge.Domain;
using backend.Modules.Badge.Domain.Constants;
using backend.Modules.Progress.Services;

namespace backend.Modules.Badge.Data;

public static class BadgeDefinitionCatalogue
{
    public static IReadOnlyList<BadgeDefinition> Definitions { get; } = CreateDefinitions();
    public static IReadOnlySet<string> RetiredCodes { get; } = new HashSet<string>(StringComparer.Ordinal);

    private static IReadOnlyList<BadgeDefinition> CreateDefinitions()
    {
        var definitions = new List<BadgeDefinition>();
        var displayOrder = 0;

        AddSeries(definitions, ref displayOrder, "workout-count", BadgeCategories.Workout,
            BadgeCriterionCodes.WorkoutCount, MetricCatalogue.WorkoutCount, "workouts", "First step",
            "Building momentum", "Centurion", "Complete 1 workout", "Complete 10 workouts", "Complete 100 workouts",
            1, 10, 100, ["workouts-1", "workouts-10", "workouts-100"]);
        AddSeries(definitions, ref displayOrder, "workout-streak", BadgeCategories.Consistency,
            BadgeCriterionCodes.WorkoutLongestStreakDays, null, "days", "Three-day rhythm", "Weekly rhythm",
            "Monthly rhythm", "Maintain a 3-day workout streak", "Maintain a 7-day workout streak",
            "Maintain a 30-day workout streak", 3, 7, 30, ["workout-streak-3", "workout-streak-7", "workout-streak-30"]);
        AddSeries(definitions, ref displayOrder, "running-distance", BadgeCategories.Distance,
            BadgeCriterionCodes.RunningDistanceTotalKm, MetricCatalogue.RunningDistanceKm, "km", "Runner's start",
            "Runner's route", "Runner's journey", "Run 25 km in total", "Run 100 km in total", "Run 500 km in total",
            25, 100, 500, ["running-distance-25", "running-distance-100", "running-distance-500"]);
        AddSeries(definitions, ref displayOrder, "cycling-distance", BadgeCategories.Distance,
            BadgeCriterionCodes.CyclingDistanceTotalKm, MetricCatalogue.CyclingDistanceKm, "km", "First century",
            "Road regular", "Long-haul rider", "Cycle 100 km in total", "Cycle 500 km in total", "Cycle 2,000 km in total",
            100, 500, 2000, ["cycling-distance-100", "cycling-distance-500", "cycling-distance-2000"]);
        AddSeries(definitions, ref displayOrder, "swimming-distance", BadgeCategories.Distance,
            BadgeCriterionCodes.SwimmingDistanceTotalMeters, MetricCatalogue.SwimmingDistanceMeters, "m", "Pool starter",
            "Pool regular", "Open-water distance", "Swim 1,000 m in total", "Swim 10,000 m in total", "Swim 50,000 m in total",
            1000, 10000, 50000, ["swimming-distance-1000", "swimming-distance-10000", "swimming-distance-50000"]);
        AddSeries(definitions, ref displayOrder, "workout-duration", BadgeCategories.Duration,
            BadgeCriterionCodes.WorkoutDurationTotalMinutes, MetricCatalogue.DurationMinutes, "minutes", "Ten hours in",
            "Fifty hours in", "Endurance archive", "Complete 600 workout minutes", "Complete 3,000 workout minutes",
            "Complete 10,000 workout minutes", 600, 3000, 10000, ["workout-duration-600", "workout-duration-3000", "workout-duration-10000"]);
        AddSeries(definitions, ref displayOrder, "gym-volume", BadgeCategories.Strength,
            BadgeCriterionCodes.GymVolumeTotalKg, MetricCatalogue.GymVolumeKg, "kg", "Volume builder", "Volume lifter",
            "Volume legend", "Lift 10,000 kg of total volume", "Lift 100,000 kg of total volume",
            "Lift 1,000,000 kg of total volume", 10000, 100000, 1000000, ["gym-volume-10000", "gym-volume-100000", "gym-volume-1000000"]);
        AddSeries(definitions, ref displayOrder, "personal-records", BadgeCategories.PersonalRecord,
            BadgeCriterionCodes.PersonalRecordAchievementCount, null, "records", "Record setter", "Record collector",
            "Record book", "Set 1 personal record", "Set 10 personal records", "Set 50 personal records", 1, 10, 50,
            ["personal-records-1", "personal-records-10", "personal-records-50"]);
        AddSeries(definitions, ref displayOrder, "goals-completed", BadgeCategories.Goal,
            BadgeCriterionCodes.GoalPeriodCompletionCount, null, "periods", "Goal reached", "Goal regular", "Goal master",
            "Complete 1 goal period", "Complete 10 goal periods", "Complete 50 goal periods", 1, 10, 50,
            ["goals-completed-1", "goals-completed-10", "goals-completed-50"]);
        AddSeries(definitions, ref displayOrder, "challenge-participation", BadgeCategories.Challenge,
            BadgeCriterionCodes.ChallengeParticipationCount, null, "results", "Challenge entrant", "Challenge regular",
            "Challenge veteran", "Finish 1 challenge", "Finish 10 challenges", "Finish 50 challenges", 1, 10, 50,
            ["challenge-participation-1", "challenge-participation-10", "challenge-participation-50"]);
        AddSeries(definitions, ref displayOrder, "challenge-target-completion", BadgeCategories.Challenge,
            BadgeCriterionCodes.ChallengeTargetCompletionCount, null, "finishes", "Challenger", "Target regular",
            "Target master", "Reach a target in 1 challenge", "Reach targets in 5 challenges", "Reach targets in 20 challenges",
            1, 5, 20, ["challenges-1", "challenges-5", "challenges-20"]);
        AddSeries(definitions, ref displayOrder, "challenge-wins", BadgeCategories.Challenge,
            BadgeCriterionCodes.ChallengeWinCount, null, "wins", "Winner", "Repeat winner", "Champion",
            "Win 1 leaderboard challenge", "Win 5 leaderboard challenges", "Win 20 leaderboard challenges", 1, 5, 20,
            ["challenge-wins-1", "challenge-wins-5", "challenge-wins-20"]);

        definitions.Add(new BadgeDefinition("workout-shares-1", "Workout shared", "Share your first completed workout.", null,
            BadgeCategories.Social, null, BadgeTiers.None, BadgeCriterionCodes.WorkoutShareCount, 1, null, "shares", ++displayOrder));
        return definitions;
    }

    private static void AddSeries(List<BadgeDefinition> definitions, ref int displayOrder, string seriesCode, string category,
        string criterionCode, string? metricCode, string unit, string bronzeName, string silverName, string goldName,
        string bronzeDescription, string silverDescription, string goldDescription, double bronzeThreshold, double silverThreshold,
        double goldThreshold, IReadOnlyList<string>? codes = null)
    {
        var resolvedCodes = codes ?? [$"{seriesCode}-bronze", $"{seriesCode}-silver", $"{seriesCode}-gold"];
        definitions.Add(new BadgeDefinition(resolvedCodes[0], bronzeName, bronzeDescription, null, category, seriesCode,
            BadgeTiers.Bronze, criterionCode, bronzeThreshold, metricCode, unit, ++displayOrder));
        definitions.Add(new BadgeDefinition(resolvedCodes[1], silverName, silverDescription, null, category, seriesCode,
            BadgeTiers.Silver, criterionCode, silverThreshold, metricCode, unit, ++displayOrder));
        definitions.Add(new BadgeDefinition(resolvedCodes[2], goldName, goldDescription, null, category, seriesCode,
            BadgeTiers.Gold, criterionCode, goldThreshold, metricCode, unit, ++displayOrder));
    }
}
