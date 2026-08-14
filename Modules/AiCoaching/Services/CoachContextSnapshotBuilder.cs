using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using backend.Data;
using backend.Modules.AiCoaching.Configuration;
using backend.Modules.AiCoaching.Domain;
using backend.Modules.BodyTracking.Domain;
using backend.Modules.Challenge.Domain;
using backend.Modules.Challenge.Domain.Constants;
using backend.Modules.Goal.Domain.Entities;
using backend.Modules.Goal.Domain.Enums;
using backend.Modules.Nutrition.Domain;
using backend.Modules.Progress.Domain;
using backend.Modules.Progress.Services;
using backend.Modules.Shared.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace backend.Modules.AiCoaching.Services;

public sealed class CoachContextSnapshotBuilder : ICoachContextSnapshotBuilder
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);
    private readonly FitspireDbContext _context;
    private readonly ICoachIntentClassifier _intentClassifier;
    private readonly AiCoachInteractionOptions _options;

    public CoachContextSnapshotBuilder(FitspireDbContext context, ICoachIntentClassifier intentClassifier,
        IOptions<AiCoachInteractionOptions> options)
    {
        _context = context;
        _intentClassifier = intentClassifier;
        _options = options.Value;
    }

    public async Task<CoachContextSnapshotBuildResult> BuildConversationAsync(Guid userId,
        CoachConversationContextRequest request, CancellationToken cancellationToken)
    {
        EnsureRequest(request.Question, request.TimeZoneId, request.RequestedAtUtc);
        var intents = _intentClassifier.Classify(request.Question);
        var window = CoachContextWindow.Create(request.TimeZoneId, request.RequestedAtUtc,
            _options.ConversationSnapshotLookbackDays, includePreviousWindow: true);
        var fitness = await BuildFitnessAsync(userId, window, intents, cancellationToken);
        var evidence = BuildEvidence(fitness);
        var snapshot = new CoachConversationContextSnapshot(CoachContextSnapshotVersions.Conversation,
            window.ToPeriod(), request.Question.Trim(), NormalizeOptionalSummary(request.ThreadSummary),
            NormalizeHistory(request.RecentMessages), intents, fitness, evidence);
        return CreateResult(snapshot, intents, evidence);
    }

    public async Task<CoachContextSnapshotBuildResult> BuildDailyBriefingAsync(Guid userId,
        CoachDailyBriefingContextRequest request, CancellationToken cancellationToken)
    {
        EnsureRequest("daily", request.TimeZoneId, request.RequestedAtUtc);
        var window = CoachContextWindow.Create(request.TimeZoneId, request.RequestedAtUtc,
            _options.DailySnapshotLookbackDays, includePreviousWindow: false);
        var intents = new[] { CoachIntent.Workout, CoachIntent.Recovery, CoachIntent.Goal, CoachIntent.BodyProgress,
            CoachIntent.Nutrition, CoachIntent.Wellbeing };
        var fitness = await BuildFitnessAsync(userId, window, intents, cancellationToken);
        var evidence = BuildEvidence(fitness);
        var snapshot = new CoachDailyBriefingContextSnapshot(CoachContextSnapshotVersions.DailyBriefing,
            window.ToPeriod(), fitness, evidence);
        return CreateResult(snapshot, intents, evidence);
    }

    private async Task<CoachFitnessContextSnapshot> BuildFitnessAsync(Guid userId, CoachContextWindow window,
        IReadOnlyCollection<CoachIntent> intents, CancellationToken cancellationToken)
    {
        var includeAll = intents.Contains(CoachIntent.GeneralFitness);
        var workouts = Includes(intents, includeAll, CoachIntent.Workout, CoachIntent.Recovery, CoachIntent.Wellbeing)
            ? await BuildWorkoutsAsync(userId, window, cancellationToken) : null;
        var goals = Includes(intents, includeAll, CoachIntent.Goal, CoachIntent.Workout, CoachIntent.Nutrition)
            ? await BuildGoalsAsync(userId, cancellationToken) : null;
        var challenges = Includes(intents, includeAll, CoachIntent.Challenge)
            ? await BuildChallengesAsync(userId, window, cancellationToken) : null;
        var body = Includes(intents, includeAll, CoachIntent.BodyProgress, CoachIntent.Recovery, CoachIntent.Wellbeing)
            ? await BuildBodyAsync(userId, window, cancellationToken) : null;
        var nutrition = Includes(intents, includeAll, CoachIntent.Nutrition, CoachIntent.Recovery, CoachIntent.Wellbeing)
            ? await BuildNutritionAsync(userId, window, cancellationToken) : null;
        return new CoachFitnessContextSnapshot(workouts, goals, challenges, body, nutrition);
    }

    private async Task<CoachWorkoutContextSnapshot> BuildWorkoutsAsync(Guid userId, CoachContextWindow window,
        CancellationToken cancellationToken)
    {
        var contributions = await _context.ActivityContributions.AsNoTracking().Where(item => item.UserId == userId &&
                item.IsActive && item.OccurredAt >= window.QueryStartUtc && item.OccurredAt < window.EndExclusiveUtc)
            .ToListAsync(cancellationToken);
        var records = await _context.PersonalRecordHistory.AsNoTracking().Where(item => item.UserId == userId &&
                item.RecordedAt >= window.StartUtc && item.RecordedAt < window.EndExclusiveUtc).CountAsync(cancellationToken);
        var current = contributions.Where(item => item.OccurredAt >= window.StartUtc).ToList();
        var previous = window.PreviousStartUtc is null ? null : contributions.Where(item => item.OccurredAt < window.StartUtc).ToList();
        var totals = CreateWorkoutTotals(current, window.TimeZone);
        return new CoachWorkoutContextSnapshot(CoverageFor(totals.WorkoutCount), totals.WorkoutCount, totals.ActiveDays,
            totals.DurationMinutes, totals.CaloriesKcal, totals.DistanceKm, totals.GymVolumeKg, records,
            CreateWorkoutTypes(current), previous is null ? null : CreateWorkoutTotals(previous, window.TimeZone));
    }

    private async Task<IReadOnlyList<CoachGoalContextSnapshot>> BuildGoalsAsync(Guid userId,
        CancellationToken cancellationToken)
    {
        var goals = await _context.Goals.AsNoTracking().Include(goal => goal.GoalType).Where(goal => goal.UserId == userId &&
                (goal.Status == GoalStatus.Active || goal.Status == GoalStatus.Completed))
            .OrderBy(goal => goal.Status).ThenBy(goal => goal.GoalType.Name).ThenBy(goal => goal.Id).Take(10).ToListAsync(cancellationToken);
        return goals.Select((goal, index) => new CoachGoalContextSnapshot($"Goal {index + 1}", goal.Unit,
            goal.Status.ToString(), Round(goal.TargetValue), Round(goal.CurrentValue), ToPercent(goal.CurrentValue, goal.TargetValue),
            goal.DefinitionKey)).ToList();
    }

    private async Task<IReadOnlyList<CoachChallengeContextSnapshot>> BuildChallengesAsync(Guid userId,
        CoachContextWindow window, CancellationToken cancellationToken)
    {
        var participants = await _context.ChallengeParticipants.AsNoTracking().Include(item => item.UserChallenge).Where(item =>
                item.UserId == userId && item.Status == ChallengeParticipantStatuses.Active &&
                item.UserChallenge.StartDate < window.EndExclusiveUtc && item.UserChallenge.EndDate >= window.StartUtc)
            .OrderBy(item => item.UserChallenge.StartDate).ThenBy(item => item.ChallengeId).Take(10).ToListAsync(cancellationToken);
        return participants.Select((participant, index) => new CoachChallengeContextSnapshot($"Challenge {index + 1}",
            participant.UserChallenge.MetricCode, participant.UserChallenge.WorkoutType, participant.UserChallenge.Mode,
            participant.UserChallenge.Status, participant.UserChallenge.TargetValue, Round(participant.Score))).ToList();
    }

    private async Task<CoachBodyContextSnapshot> BuildBodyAsync(Guid userId, CoachContextWindow window,
        CancellationToken cancellationToken)
    {
        var entries = await _context.BodyCheckIns.AsNoTracking().Where(item => item.UserId == userId && item.DeletedAt == null &&
                item.CheckInDate >= window.StartDate && item.CheckInDate <= window.EndDate)
            .OrderBy(item => item.CheckInDate).ThenBy(item => item.Id).ToListAsync(cancellationToken);
        var wellbeingValues = entries.Where(item => item.WellbeingScore.HasValue).Select(item => item.WellbeingScore!.Value).ToList();
        return new CoachBodyContextSnapshot(CoverageFor(entries.Count, partialAt: 2), entries.Count,
            CreateTrend(entries, item => item.WeightKg), CreateTrend(entries, item => item.BodyFatPercent),
            CreateTrend(entries, item => item.WaistCm), wellbeingValues.LastOrDefault() is var latest && latest > 0 ? latest : null,
            wellbeingValues.Count == 0 ? null : Round(wellbeingValues.Average()));
    }

    private async Task<CoachNutritionContextSnapshot> BuildNutritionAsync(Guid userId, CoachContextWindow window,
        CancellationToken cancellationToken)
    {
        var meals = await _context.Meals.AsNoTracking().Include(meal => meal.Items.Where(item => item.DeletedAt == null)).Where(meal =>
                meal.UserId == userId && meal.DeletedAt == null && meal.MealDate >= window.StartDate && meal.MealDate <= window.EndDate)
            .ToListAsync(cancellationToken);
        var target = await _context.NutritionTargets.AsNoTracking().FirstOrDefaultAsync(item => item.UserId == userId, cancellationToken);
        var nutrition = CreateNutritionSummary(meals);
        var targets = target is null ? null : new CoachNutritionTargets(target.CaloriesKcal, target.ProteinGrams,
            target.CarbsGrams, target.FatGrams);
        return new CoachNutritionContextSnapshot(CoverageFor(nutrition.LoggedDays, partialAt: 4), nutrition.LoggedDays,
            nutrition.Average, targets, CreatePercentages(nutrition.Average, targets));
    }

    private static bool Includes(IReadOnlyCollection<CoachIntent> intents, bool includeAll, params CoachIntent[] candidates) =>
        includeAll || candidates.Any(intents.Contains);

    private static CoachWorkoutTotals CreateWorkoutTotals(IReadOnlyCollection<ActivityContribution> items, TimeZoneInfo timeZone)
    {
        var workoutItems = items.Where(item => item.MetricCode == MetricCatalogue.WorkoutCount).ToList();
        var activeDays = workoutItems.Select(item => DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(
                DateTime.SpecifyKind(item.OccurredAt, DateTimeKind.Utc), timeZone))).Distinct().Count();
        return new CoachWorkoutTotals(workoutItems.Count, activeDays, SumMetric(items, MetricCatalogue.DurationMinutes),
            SumMetric(items, MetricCatalogue.CaloriesKcal), Round(SumMetric(items, MetricCatalogue.RunningDistanceKm) +
            SumMetric(items, MetricCatalogue.CyclingDistanceKm) + SumMetric(items, MetricCatalogue.SwimmingDistanceMeters) / 1_000d),
            SumMetric(items, MetricCatalogue.GymVolumeKg));
    }

    private static IReadOnlyList<CoachWorkoutTypeCount> CreateWorkoutTypes(IReadOnlyCollection<ActivityContribution> items) =>
        items.Where(item => item.MetricCode == MetricCatalogue.WorkoutCount).GroupBy(item => item.WorkoutType.Trim().ToLowerInvariant())
            .OrderBy(group => group.Key).Select(group => new CoachWorkoutTypeCount(group.Key, group.Count())).ToList();

    private static CoachMeasurementTrend CreateTrend(IEnumerable<BodyCheckIn> entries, Func<BodyCheckIn, double?> selector)
    {
        var values = entries.Select(selector).Where(value => value.HasValue).Select(value => value!.Value).ToList();
        return values.Count switch
        {
            0 => new CoachMeasurementTrend(null, null, null),
            1 => new CoachMeasurementTrend(Round(values[0]), Round(values[0]), null),
            _ => new CoachMeasurementTrend(Round(values[0]), Round(values[^1]), Round(values[^1] - values[0]))
        };
    }

    private static CoachNutritionSummary CreateNutritionSummary(IEnumerable<Meal> meals)
    {
        var totals = meals.GroupBy(meal => meal.MealDate).Select(group => group.Select(item => item.CalculateTotals()).Aggregate(
            new CoachNutritionTotals(0, 0, 0, 0), (current, item) => new CoachNutritionTotals(
                current.CaloriesKcal + item.CaloriesKcal, current.ProteinGrams + item.ProteinGrams,
                current.CarbsGrams + item.CarbsGrams, current.FatGrams + item.FatGrams))).ToList();
        if (totals.Count == 0)
            return new CoachNutritionSummary(0, null);

        return new CoachNutritionSummary(totals.Count, new CoachNutritionTotals(decimal.Round(totals.Average(item => item.CaloriesKcal), 2),
            decimal.Round(totals.Average(item => item.ProteinGrams), 2), decimal.Round(totals.Average(item => item.CarbsGrams), 2),
            decimal.Round(totals.Average(item => item.FatGrams), 2)));
    }

    private static CoachNutritionPercentages? CreatePercentages(CoachNutritionTotals? average, CoachNutritionTargets? targets) =>
        average is null || targets is null ? null : new CoachNutritionPercentages(Percent(average.CaloriesKcal, targets.CaloriesKcal),
            Percent(average.ProteinGrams, targets.ProteinGrams), Percent(average.CarbsGrams, targets.CarbsGrams),
            Percent(average.FatGrams, targets.FatGrams));

    private static IReadOnlyList<CoachEvidence> BuildEvidence(CoachFitnessContextSnapshot fitness)
    {
        var evidence = new List<CoachEvidence>();
        AddWorkoutEvidence(evidence, fitness.Workouts);
        AddGoalEvidence(evidence, fitness.Goals);
        AddChallengeEvidence(evidence, fitness.Challenges);
        AddBodyEvidence(evidence, fitness.Body);
        AddNutritionEvidence(evidence, fitness.Nutrition);
        return evidence;
    }

    private static void AddWorkoutEvidence(ICollection<CoachEvidence> evidence, CoachWorkoutContextSnapshot? workouts)
    {
        if (workouts is null)
            return;
        evidence.Add(Metric("workouts.count", "Completed workouts", workouts.WorkoutCount));
        evidence.Add(Metric("workouts.active-days", "Active workout days", workouts.ActiveDays));
        evidence.Add(Metric("workouts.duration-minutes", "Workout duration in minutes", workouts.DurationMinutes));
        evidence.Add(Metric("workouts.calories-kcal", "Workout calories burned", workouts.CaloriesKcal));
        evidence.Add(Metric("workouts.distance-km", "Workout distance in kilometres", workouts.DistanceKm));
        evidence.Add(Metric("workouts.gym-volume-kg", "Gym volume in kilograms", workouts.GymVolumeKg));
        evidence.Add(Metric("workouts.personal-records", "Personal records", workouts.PersonalRecordCount));
    }

    private static void AddGoalEvidence(ICollection<CoachEvidence> evidence, IReadOnlyList<CoachGoalContextSnapshot>? goals)
    {
        if (goals is null)
            return;
        foreach (var (goal, index) in goals.Select((goal, index) => (goal, index)))
            evidence.Add(Metric($"goals.{index + 1}.progress", $"{goal.Label} progress percentage", goal.ProgressPercent));
    }

    private static void AddChallengeEvidence(ICollection<CoachEvidence> evidence,
        IReadOnlyList<CoachChallengeContextSnapshot>? challenges)
    {
        if (challenges is null)
            return;
        foreach (var (challenge, index) in challenges.Select((challenge, index) => (challenge, index)))
            evidence.Add(Metric($"challenges.{index + 1}.score", $"{challenge.Label} score", challenge.Score));
    }

    private static void AddBodyEvidence(ICollection<CoachEvidence> evidence, CoachBodyContextSnapshot? body)
    {
        if (body is null)
            return;
        AddTrendEvidence(evidence, "body.weight-kg", "Weight in kilograms", body.WeightKg);
        AddTrendEvidence(evidence, "body.body-fat-percent", "Body fat percentage", body.BodyFatPercent);
        AddTrendEvidence(evidence, "body.waist-cm", "Waist circumference in centimetres", body.WaistCm);
        if (body.LatestWellbeingScore.HasValue)
            evidence.Add(Metric("body.wellbeing-latest", "Latest wellbeing score", body.LatestWellbeingScore.Value));
        if (body.AverageWellbeingScore.HasValue)
            evidence.Add(Metric("body.wellbeing-average", "Average wellbeing score", body.AverageWellbeingScore.Value));
    }

    private static void AddNutritionEvidence(ICollection<CoachEvidence> evidence, CoachNutritionContextSnapshot? nutrition)
    {
        if (nutrition is null)
            return;
        evidence.Add(Metric("nutrition.logged-days", "Nutrition logged days", nutrition.LoggedDays));
        if (nutrition.AveragePerLoggedDay is null)
            return;
        evidence.Add(Metric("nutrition.calories-average", "Average logged calories per day", nutrition.AveragePerLoggedDay.CaloriesKcal));
        evidence.Add(Metric("nutrition.protein-average", "Average logged protein grams per day", nutrition.AveragePerLoggedDay.ProteinGrams));
        evidence.Add(Metric("nutrition.carbs-average", "Average logged carbohydrate grams per day", nutrition.AveragePerLoggedDay.CarbsGrams));
        evidence.Add(Metric("nutrition.fat-average", "Average logged fat grams per day", nutrition.AveragePerLoggedDay.FatGrams));
    }

    private static void AddTrendEvidence(ICollection<CoachEvidence> evidence, string key, string description,
        CoachMeasurementTrend trend)
    {
        if (trend.LastValue.HasValue)
            evidence.Add(Metric(key, description, trend.LastValue.Value));
        if (trend.Change.HasValue)
            evidence.Add(Metric($"{key}.change", $"{description} change during the selected period", trend.Change.Value));
    }

    private static CoachContextSnapshotBuildResult CreateResult<T>(T snapshot, IReadOnlyList<CoachIntent> intents,
        IReadOnlyList<CoachEvidence> evidence)
    {
        var json = JsonSerializer.Serialize(snapshot, SerializerOptions);
        var fingerprint = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(json))).ToLowerInvariant();
        return new CoachContextSnapshotBuildResult(json, fingerprint, evidence.Select(item => item.Key).ToHashSet(StringComparer.Ordinal), intents);
    }

    private static void EnsureRequest(string question, string timeZoneId, DateTime utcNow)
    {
        if (string.IsNullOrWhiteSpace(question) || question.Trim().Length > AiCoachInteractionLimits.MaximumQuestionLength)
            throw new ArgumentException("Coach question is required and must be within the allowed length.", nameof(question));
        if (string.IsNullOrWhiteSpace(timeZoneId))
            throw new ArgumentException("Timezone is required.", nameof(timeZoneId));
        AiCoachDomainRules.EnsureUtc(utcNow, nameof(utcNow));
    }

    private IReadOnlyList<CoachConversationHistoryMessage> NormalizeHistory(
        IReadOnlyList<CoachConversationHistoryMessage> messages)
    {
        if (messages.Count > _options.ConversationContextMessageLimit)
            throw new ArgumentException("Conversation context exceeds the allowed message limit.", nameof(messages));
        return messages.Select(NormalizeHistoryMessage).ToList();
    }

    private static CoachConversationHistoryMessage NormalizeHistoryMessage(CoachConversationHistoryMessage message)
    {
        var role = AiCoachDomainRules.NormalizeRequired(message.Role, 20, "Conversation role");
        if (role is not ("User" or "Assistant"))
            throw new DomainException("Conversation role is invalid.");

        return new CoachConversationHistoryMessage(role, AiCoachDomainRules.NormalizeRequired(message.Content,
            AiCoachInteractionLimits.MaximumAnswerMarkdownLength, "Conversation content"));
    }

    private static string? NormalizeOptionalSummary(string? summary) => string.IsNullOrWhiteSpace(summary) ? null :
        AiCoachDomainRules.NormalizeRequired(summary, AiCoachInteractionLimits.MaximumThreadSummaryLength, "Thread summary");

    private static CoachSectionCoverage CoverageFor(int count, int partialAt = 1) =>
        count == 0 ? new CoachSectionCoverage(CoachSnapshotCoverageState.Unavailable, 0) :
        count < partialAt ? new CoachSectionCoverage(CoachSnapshotCoverageState.Partial, count) :
        new CoachSectionCoverage(CoachSnapshotCoverageState.Sufficient, count);

    private static double SumMetric(IEnumerable<ActivityContribution> items, string metricCode) =>
        Round(items.Where(item => item.MetricCode == metricCode).Sum(item => item.Value));

    private static int ToPercent(double currentValue, double targetValue) => targetValue <= 0 ? 0 :
        Math.Clamp((int)Math.Round(currentValue / targetValue * 100, MidpointRounding.AwayFromZero), 0, 100);

    private static decimal? Percent(decimal value, decimal? target) => target is > 0 ? decimal.Round(value / target.Value * 100, 2) : null;

    private static CoachEvidence Metric(string key, string description, object value) =>
        new(key, $"{description}: {Convert.ToString(value, CultureInfo.InvariantCulture)}.");

    private static double Round(double value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);

    private sealed record CoachNutritionSummary(int LoggedDays, CoachNutritionTotals? Average);
}

internal sealed record CoachContextWindow(DateOnly StartDate, DateOnly EndDate, DateTime StartUtc,
    DateTime EndExclusiveUtc, DateTime? PreviousStartUtc, TimeZoneInfo TimeZone)
{
    public DateTime QueryStartUtc => PreviousStartUtc ?? StartUtc;

    public static CoachContextWindow Create(string timeZoneId, DateTime utcNow, int durationDays, bool includePreviousWindow)
    {
        var timeZone = ResolveTimeZone(timeZoneId);
        var localEndDate = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(utcNow, timeZone));
        var startDate = localEndDate.AddDays(-(durationDays - 1));
        var endExclusiveUtc = ToUtc(localEndDate.AddDays(1), timeZone);
        var startUtc = ToUtc(startDate, timeZone);
        return new CoachContextWindow(startDate, localEndDate, startUtc, endExclusiveUtc,
            includePreviousWindow ? ToUtc(startDate.AddDays(-durationDays), timeZone) : null, timeZone);
    }

    public CoachSnapshotPeriod ToPeriod() => new(StartDate, EndDate, TimeZone.Id);

    private static TimeZoneInfo ResolveTimeZone(string timeZoneId)
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId.Trim());
        }
        catch (TimeZoneNotFoundException exception)
        {
            throw new DomainException("User timezone is not supported.", exception);
        }
        catch (InvalidTimeZoneException exception)
        {
            throw new DomainException("User timezone is invalid.", exception);
        }
    }

    private static DateTime ToUtc(DateOnly date, TimeZoneInfo timeZone) => TimeZoneInfo.ConvertTimeToUtc(
        DateTime.SpecifyKind(date.ToDateTime(TimeOnly.MinValue), DateTimeKind.Unspecified), timeZone);
}
