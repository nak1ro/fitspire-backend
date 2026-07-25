using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using backend.Data;
using backend.Modules.Challenge.Domain;
using backend.Modules.Goal.Domain.Entities;
using backend.Modules.Goal.Domain.Enums;
using backend.Modules.Nutrition.Domain;
using backend.Modules.Progress.Domain;
using backend.Modules.Progress.Services;
using backend.Modules.Workout.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace backend.Modules.AiCoaching.Services;

public sealed class WeeklyCoachSnapshotBuilder : IWeeklyCoachSnapshotBuilder
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);
    private readonly FitspireDbContext _context;

    public WeeklyCoachSnapshotBuilder(FitspireDbContext context)
    {
        _context = context;
    }

    public async Task<WeeklyCoachSnapshotBuildResult> BuildAsync(Guid userId, WeeklyCoachPeriod period,
        CancellationToken cancellationToken)
    {
        var workouts = await BuildWorkoutsAsync(userId, period, cancellationToken);
        var goals = await BuildGoalsAsync(userId, period, cancellationToken);
        var challenges = await BuildChallengesAsync(userId, period, cancellationToken);
        var body = await BuildBodyAsync(userId, period, cancellationToken);
        var nutrition = await BuildNutritionAsync(userId, period, cancellationToken);
        var evidence = BuildEvidence(workouts, goals, challenges, body, nutrition);
        var snapshot = new WeeklyCoachSnapshot(WeeklyCoachSnapshotVersions.Snapshot,
            new WeeklyCoachSnapshotPeriod(period.PeriodStart, period.PeriodEnd, period.TimeZoneId),
            CreateCoverage(workouts, goals, challenges, body, nutrition), workouts, goals, challenges, body, nutrition, evidence);
        var json = JsonSerializer.Serialize(snapshot, SerializerOptions);
        var fingerprint = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(json))).ToLowerInvariant();
        return new WeeklyCoachSnapshotBuildResult(snapshot, json, fingerprint,
            evidence.Select(item => item.Key).ToHashSet(StringComparer.Ordinal));
    }

    private async Task<WeeklyCoachWorkoutSnapshot> BuildWorkoutsAsync(Guid userId, WeeklyCoachPeriod period,
        CancellationToken cancellationToken)
    {
        var previous = period.Previous();
        var contributions = await _context.ActivityContributions.AsNoTracking()
            .Where(item => item.UserId == userId && item.IsActive && item.OccurredAt >= previous.StartAtUtc &&
                           item.OccurredAt < period.EndExclusiveAtUtc)
            .ToListAsync(cancellationToken);
        var records = await _context.PersonalRecordHistory.AsNoTracking()
            .Where(item => item.UserId == userId && item.RecordedAt >= period.StartAtUtc && item.RecordedAt < period.EndExclusiveAtUtc)
            .CountAsync(cancellationToken);
        var currentItems = contributions.Where(item => item.OccurredAt >= period.StartAtUtc).ToList();
        var previousItems = contributions.Where(item => item.OccurredAt < period.StartAtUtc).ToList();
        var current = CreateWorkoutTotals(currentItems, period.TimeZoneId);
        return new WeeklyCoachWorkoutSnapshot(current.WorkoutCount, current.ActiveDays, current.DurationMinutes,
            current.CaloriesKcal, current.DistanceKm, current.GymVolumeKg, records,
            CreateWorkoutTypes(currentItems), CreateWorkoutTotals(previousItems, period.TimeZoneId));
    }

    private static WeeklyCoachWorkoutTotals CreateWorkoutTotals(IReadOnlyCollection<ActivityContribution> items,
        string timeZoneId)
    {
        var workoutItems = items.Where(item => item.MetricCode == MetricCatalogue.WorkoutCount).ToList();
        var zone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        var activeDays = workoutItems.Select(item => DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(
                DateTime.SpecifyKind(item.OccurredAt, DateTimeKind.Utc), zone)))
            .Distinct().Count();
        return new WeeklyCoachWorkoutTotals(workoutItems.Count, activeDays,
            SumMetric(items, MetricCatalogue.DurationMinutes), SumMetric(items, MetricCatalogue.CaloriesKcal),
            Round(SumMetric(items, MetricCatalogue.RunningDistanceKm) + SumMetric(items, MetricCatalogue.CyclingDistanceKm) +
                  SumMetric(items, MetricCatalogue.SwimmingDistanceMeters) / 1000d),
            SumMetric(items, MetricCatalogue.GymVolumeKg));
    }

    private static IReadOnlyList<WeeklyCoachWorkoutTypeCount> CreateWorkoutTypes(IReadOnlyCollection<ActivityContribution> items) =>
        items.Where(item => item.MetricCode == MetricCatalogue.WorkoutCount)
            .GroupBy(item => item.WorkoutType.Trim().ToLowerInvariant())
            .OrderBy(group => group.Key).Select(group => new WeeklyCoachWorkoutTypeCount(group.Key, group.Count())).ToList();

    private async Task<IReadOnlyList<WeeklyCoachGoalSnapshot>> BuildGoalsAsync(Guid userId, WeeklyCoachPeriod period,
        CancellationToken cancellationToken)
    {
        var goals = await _context.Goals.AsNoTracking().Include(goal => goal.GoalType)
            .Include(goal => goal.ProgressEntries.Where(entry => entry.RecordedAt >= period.StartAtUtc && entry.RecordedAt < period.EndExclusiveAtUtc))
            .Where(goal => goal.UserId == userId && (goal.Status == GoalStatus.Active || goal.Status == GoalStatus.Completed) &&
                           goal.StartDate < period.EndExclusiveAtUtc && (goal.Deadline == null || goal.Deadline >= period.StartAtUtc))
            .OrderBy(goal => goal.GoalType.Name).ThenBy(goal => goal.Id).Take(10).ToListAsync(cancellationToken);

        return goals.Select(goal =>
        {
            var entries = goal.ProgressEntries.OrderBy(entry => entry.RecordedAt).ToList();
            return new WeeklyCoachGoalSnapshot(goal.GoalType.Name, goal.Unit, goal.Status.ToString(), Round(goal.TargetValue),
                entries.LastOrDefault()?.NewValue is { } progress ? Round(progress) : null,
                Round(entries.Sum(entry => entry.Delta)), entries.Count > 0);
        }).ToList();
    }

    private async Task<IReadOnlyList<WeeklyCoachChallengeSnapshot>> BuildChallengesAsync(Guid userId, WeeklyCoachPeriod period,
        CancellationToken cancellationToken)
    {
        var participants = await _context.ChallengeParticipants.AsNoTracking().Include(item => item.UserChallenge)
            .Where(item => item.UserId == userId && item.UserChallenge.StartDate < period.EndExclusiveAtUtc &&
                           item.UserChallenge.EndDate >= period.StartAtUtc)
            .OrderBy(item => item.UserChallenge.StartDate).ThenBy(item => item.ChallengeId).Take(10).ToListAsync(cancellationToken);
        if (participants.Count == 0)
            return [];

        var participantIds = participants.Select(item => item.Id).ToList();
        var periodProgress = await (from score in _context.ChallengeScoreContributions.AsNoTracking()
                                    join activity in _context.ActivityContributions.AsNoTracking() on score.ActivityContributionId equals activity.Id
                                    where participantIds.Contains(score.ParticipantId) && activity.IsActive &&
                                          activity.OccurredAt >= period.StartAtUtc && activity.OccurredAt < period.EndExclusiveAtUtc
                                    group score by score.ParticipantId into groupByParticipant
                                    select new { ParticipantId = groupByParticipant.Key, Value = groupByParticipant.Sum(item => item.Value) })
            .ToDictionaryAsync(item => item.ParticipantId, item => item.Value, cancellationToken);
        var results = await _context.ChallengeResults.AsNoTracking().Where(item => item.UserId == userId &&
                participantIds.Contains(item.ParticipantId) && item.FinalizedAt >= period.StartAtUtc && item.FinalizedAt < period.EndExclusiveAtUtc)
            .ToDictionaryAsync(item => item.ParticipantId, cancellationToken);

        return participants.Select((participant, index) =>
        {
            results.TryGetValue(participant.Id, out var result);
            return new WeeklyCoachChallengeSnapshot($"Challenge {index + 1}", participant.UserChallenge.MetricCode,
                participant.UserChallenge.WorkoutType, participant.UserChallenge.Mode, participant.UserChallenge.Status,
                participant.UserChallenge.TargetValue, Round(periodProgress.GetValueOrDefault(participant.Id)),
                result?.IsFinisher, result?.Rank);
        }).ToList();
    }

    private async Task<WeeklyCoachBodySnapshot> BuildBodyAsync(Guid userId, WeeklyCoachPeriod period,
        CancellationToken cancellationToken)
    {
        var previous = period.Previous();
        var entries = await _context.BodyCheckIns.AsNoTracking().Where(item => item.UserId == userId && item.DeletedAt == null &&
                item.CheckInDate >= previous.PeriodStart && item.CheckInDate <= period.PeriodEnd)
            .OrderBy(item => item.CheckInDate).ThenBy(item => item.Id).ToListAsync(cancellationToken);
        var current = entries.Where(item => item.CheckInDate >= period.PeriodStart).ToList();
        var previousEntries = entries.Where(item => item.CheckInDate < period.PeriodStart).ToList();
        return new WeeklyCoachBodySnapshot(current.Count, CreateTrend(current, item => item.WeightKg),
            CreateTrend(current, item => item.BodyFatPercent), CreateTrend(current, item => item.WaistCm),
            AverageWellbeing(current), AverageWellbeing(previousEntries));
    }

    private async Task<WeeklyCoachNutritionSnapshot> BuildNutritionAsync(Guid userId, WeeklyCoachPeriod period,
        CancellationToken cancellationToken)
    {
        var previous = period.Previous();
        var meals = await _context.Meals.AsNoTracking().Include(meal => meal.Items.Where(item => item.DeletedAt == null))
            .Where(meal => meal.UserId == userId && meal.DeletedAt == null && meal.MealDate >= previous.PeriodStart &&
                           meal.MealDate <= period.PeriodEnd).ToListAsync(cancellationToken);
        var target = await _context.NutritionTargets.AsNoTracking().FirstOrDefaultAsync(item => item.UserId == userId, cancellationToken);
        var current = CreateNutritionWeek(meals.Where(meal => meal.MealDate >= period.PeriodStart));
        var previousWeek = CreateNutritionWeek(meals.Where(meal => meal.MealDate < period.PeriodStart));
        var targets = target is null ? null : new WeeklyCoachNutritionTargets(target.CaloriesKcal, target.ProteinGrams,
            target.CarbsGrams, target.FatGrams);
        return new WeeklyCoachNutritionSnapshot(current.LoggedDays, current.Average, previousWeek.Average, targets,
            CreatePercentages(current.Average, targets));
    }

    private static WeeklyCoachSnapshotCoverage CreateCoverage(WeeklyCoachWorkoutSnapshot workouts,
        IReadOnlyList<WeeklyCoachGoalSnapshot> goals, IReadOnlyList<WeeklyCoachChallengeSnapshot> challenges,
        WeeklyCoachBodySnapshot body, WeeklyCoachNutritionSnapshot nutrition) => new(
        new WeeklyCoachSectionCoverage(workouts.WorkoutCount == 0 ? WeeklyCoachCoverage.Unavailable : WeeklyCoachCoverage.Sufficient, workouts.WorkoutCount),
        new WeeklyCoachSectionCoverage(goals.Count == 0 ? WeeklyCoachCoverage.Unavailable : WeeklyCoachCoverage.Sufficient, goals.Count),
        new WeeklyCoachSectionCoverage(challenges.Count == 0 ? WeeklyCoachCoverage.Unavailable : WeeklyCoachCoverage.Sufficient, challenges.Count),
        new WeeklyCoachSectionCoverage(body.CheckInCount switch { 0 => WeeklyCoachCoverage.Unavailable, 1 => WeeklyCoachCoverage.Partial, _ => WeeklyCoachCoverage.Sufficient }, body.CheckInCount),
        new WeeklyCoachSectionCoverage(nutrition.LoggedDayCount switch { 0 => WeeklyCoachCoverage.Unavailable, < 4 => WeeklyCoachCoverage.Partial, _ => WeeklyCoachCoverage.Sufficient }, nutrition.LoggedDayCount));

    private static IReadOnlyList<WeeklyCoachEvidence> BuildEvidence(WeeklyCoachWorkoutSnapshot workouts,
        IReadOnlyList<WeeklyCoachGoalSnapshot> goals, IReadOnlyList<WeeklyCoachChallengeSnapshot> challenges,
        WeeklyCoachBodySnapshot body, WeeklyCoachNutritionSnapshot nutrition)
    {
        var evidence = new List<WeeklyCoachEvidence>
        {
            Metric("workouts.count", "Completed workouts", workouts.WorkoutCount),
            Metric("workouts.active-days", "Active workout days", workouts.ActiveDays),
            Metric("workouts.duration-minutes", "Workout duration in minutes", workouts.DurationMinutes),
            Metric("workouts.calories-kcal", "Workout calories burned", workouts.CaloriesKcal),
            Metric("workouts.distance-km", "Workout distance in kilometres", workouts.DistanceKm),
            Metric("workouts.gym-volume-kg", "Gym volume in kilograms", workouts.GymVolumeKg),
            Metric("workouts.personal-records", "Personal records", workouts.PersonalRecordCount)
        };
        evidence.AddRange(goals.Select((goal, index) => Metric($"goals.{index + 1}.period-progress", $"{goal.Name} progress during the report period", goal.ProgressDeltaDuringPeriod)));
        evidence.AddRange(challenges.Select((challenge, index) => Metric($"challenges.{index + 1}.period-progress", $"{challenge.Label} progress during the report period", challenge.ProgressDuringPeriod)));
        AddMeasurementEvidence(evidence, "body.weight-kg", "Weight in kilograms", body.WeightKg);
        AddMeasurementEvidence(evidence, "body.body-fat-percent", "Body fat percentage", body.BodyFatPercent);
        AddMeasurementEvidence(evidence, "body.waist-cm", "Waist circumference in centimetres", body.WaistCm);
        AddOptionalEvidence(evidence, "body.wellbeing-average", "Average wellbeing score", body.AverageWellbeingScore);
        AddNutritionEvidence(evidence, nutrition);
        return evidence;
    }

    private static double SumMetric(IEnumerable<ActivityContribution> items, string metricCode) =>
        Round(items.Where(item => item.MetricCode == metricCode).Sum(item => item.Value));

    private static WeeklyCoachMeasurementTrend CreateTrend(IEnumerable<backend.Modules.BodyTracking.Domain.BodyCheckIn> entries,
        Func<backend.Modules.BodyTracking.Domain.BodyCheckIn, double?> selector)
    {
        var values = entries.Select(selector).Where(value => value.HasValue).Select(value => value!.Value).ToList();
        return values.Count switch
        {
            0 => new WeeklyCoachMeasurementTrend(null, null, null),
            1 => new WeeklyCoachMeasurementTrend(Round(values[0]), Round(values[0]), null),
            _ => new WeeklyCoachMeasurementTrend(Round(values[0]), Round(values[^1]), Round(values[^1] - values[0]))
        };
    }

    private static double? AverageWellbeing(IEnumerable<backend.Modules.BodyTracking.Domain.BodyCheckIn> entries)
    {
        var scores = entries.Where(item => item.WellbeingScore.HasValue).Select(item => item.WellbeingScore!.Value).ToList();
        return scores.Count == 0 ? null : Round(scores.Average());
    }

    private static WeeklyCoachNutritionWeek CreateNutritionWeek(IEnumerable<Meal> meals)
    {
        var totals = meals.GroupBy(meal => meal.MealDate).Select(group => group.Select(item => item.CalculateTotals()).Aggregate(
            new WeeklyCoachNutritionTotals(0, 0, 0, 0),
            (current, item) => new WeeklyCoachNutritionTotals(current.CaloriesKcal + item.CaloriesKcal,
                current.ProteinGrams + item.ProteinGrams, current.CarbsGrams + item.CarbsGrams, current.FatGrams + item.FatGrams))).ToList();
        if (totals.Count == 0)
            return new WeeklyCoachNutritionWeek(0, null);

        return new WeeklyCoachNutritionWeek(totals.Count, new WeeklyCoachNutritionTotals(
            decimal.Round(totals.Average(item => item.CaloriesKcal), 2), decimal.Round(totals.Average(item => item.ProteinGrams), 2),
            decimal.Round(totals.Average(item => item.CarbsGrams), 2), decimal.Round(totals.Average(item => item.FatGrams), 2)));
    }

    private static WeeklyCoachNutritionPercentages? CreatePercentages(WeeklyCoachNutritionTotals? average,
        WeeklyCoachNutritionTargets? targets) => average is null || targets is null ? null : new WeeklyCoachNutritionPercentages(
        Percent(average.CaloriesKcal, targets.CaloriesKcal), Percent(average.ProteinGrams, targets.ProteinGrams),
        Percent(average.CarbsGrams, targets.CarbsGrams), Percent(average.FatGrams, targets.FatGrams));

    private static decimal? Percent(decimal value, decimal? target) => target is > 0 ? decimal.Round(value / target.Value * 100, 2) : null;

    private static void AddMeasurementEvidence(List<WeeklyCoachEvidence> evidence, string key, string description,
        WeeklyCoachMeasurementTrend trend)
    {
        if (trend.LastValue.HasValue)
            evidence.Add(Metric(key, description, trend.LastValue.Value));
        if (trend.Change.HasValue)
            evidence.Add(Metric($"{key}.change", $"{description} change during the report period", trend.Change.Value));
    }

    private static void AddOptionalEvidence(List<WeeklyCoachEvidence> evidence, string key, string description, double? value)
    {
        if (value.HasValue)
            evidence.Add(Metric(key, description, value.Value));
    }

    private static void AddNutritionEvidence(List<WeeklyCoachEvidence> evidence, WeeklyCoachNutritionSnapshot nutrition)
    {
        evidence.Add(Metric("nutrition.logged-days", "Nutrition logged days", nutrition.LoggedDayCount));
        if (nutrition.AveragePerLoggedDay is null)
            return;

        evidence.Add(Metric("nutrition.calories-average", "Average logged calories per day", nutrition.AveragePerLoggedDay.CaloriesKcal));
        evidence.Add(Metric("nutrition.protein-average", "Average logged protein grams per day", nutrition.AveragePerLoggedDay.ProteinGrams));
        evidence.Add(Metric("nutrition.carbs-average", "Average logged carbohydrate grams per day", nutrition.AveragePerLoggedDay.CarbsGrams));
        evidence.Add(Metric("nutrition.fat-average", "Average logged fat grams per day", nutrition.AveragePerLoggedDay.FatGrams));
    }

    private static WeeklyCoachEvidence Metric(string key, string description, object value) =>
        new(key, $"{description}: {Convert.ToString(value, CultureInfo.InvariantCulture)}.");

    private static double Round(double value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);

    private sealed record WeeklyCoachNutritionWeek(int LoggedDays, WeeklyCoachNutritionTotals? Average);
}
