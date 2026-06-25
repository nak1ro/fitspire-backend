using backend.Data;
using backend.Modules.Badge.Domain.Constants;
using backend.Modules.Progress.Services;
using backend.Modules.Social.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace backend.Modules.Badge.Services;

public interface IBadgeAchievementSnapshotService
{
    Task<BadgeAchievementSnapshot> CreateAsync(Guid userId, CancellationToken cancellationToken = default);
}

public class BadgeAchievementSnapshotService : IBadgeAchievementSnapshotService
{
    private const string DefaultTimeZoneId = "Central European Standard Time";
    private readonly FitspireDbContext _context;

    public BadgeAchievementSnapshotService(FitspireDbContext context)
    {
        _context = context;
    }

    public async Task<BadgeAchievementSnapshot> CreateAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var totals = await GetContributionTotalsAsync(userId, cancellationToken);
        var streak = await GetLongestWorkoutStreakAsync(userId, cancellationToken);
        var personalRecordCount = await GetPersonalRecordAchievementCountAsync(userId, cancellationToken);
        var goalCount = await _context.GoalPeriods.AsNoTracking().CountAsync(period => period.Goal.UserId == userId && period.Status == "Completed", cancellationToken);
        var challengeCounts = await GetChallengeCountsAsync(userId, cancellationToken);
        var shareCount = await _context.Posts.AsNoTracking().CountAsync(post => post.UserId == userId && post.Type == PostType.WorkoutShare, cancellationToken);

        return new BadgeAchievementSnapshot(new Dictionary<string, double>(StringComparer.Ordinal)
        {
            [BadgeCriterionCodes.WorkoutCount] = GetTotal(totals, MetricCatalogue.WorkoutCount),
            [BadgeCriterionCodes.WorkoutLongestStreakDays] = streak,
            [BadgeCriterionCodes.RunningDistanceTotalKm] = GetTotal(totals, MetricCatalogue.RunningDistanceKm),
            [BadgeCriterionCodes.CyclingDistanceTotalKm] = GetTotal(totals, MetricCatalogue.CyclingDistanceKm),
            [BadgeCriterionCodes.SwimmingDistanceTotalMeters] = GetTotal(totals, MetricCatalogue.SwimmingDistanceMeters),
            [BadgeCriterionCodes.WorkoutDurationTotalMinutes] = GetTotal(totals, MetricCatalogue.DurationMinutes),
            [BadgeCriterionCodes.GymVolumeTotalKg] = GetTotal(totals, MetricCatalogue.GymVolumeKg),
            [BadgeCriterionCodes.PersonalRecordAchievementCount] = personalRecordCount,
            [BadgeCriterionCodes.GoalPeriodCompletionCount] = goalCount,
            [BadgeCriterionCodes.ChallengeParticipationCount] = challengeCounts.Participation,
            [BadgeCriterionCodes.ChallengeTargetCompletionCount] = challengeCounts.TargetCompletions,
            [BadgeCriterionCodes.ChallengeWinCount] = challengeCounts.Wins,
            [BadgeCriterionCodes.WorkoutShareCount] = shareCount
        });
    }

    private async Task<IReadOnlyDictionary<string, double>> GetContributionTotalsAsync(Guid userId, CancellationToken cancellationToken)
    {
        var metricCodes = new[]
        {
            MetricCatalogue.WorkoutCount, MetricCatalogue.RunningDistanceKm, MetricCatalogue.CyclingDistanceKm,
            MetricCatalogue.SwimmingDistanceMeters, MetricCatalogue.DurationMinutes, MetricCatalogue.GymVolumeKg
        };
        var totals = await _context.ActivityContributions.AsNoTracking().Where(contribution => contribution.UserId == userId &&
                contribution.IsActive && metricCodes.Contains(contribution.MetricCode))
            .GroupBy(contribution => contribution.MetricCode)
            .Select(group => new { MetricCode = group.Key, Value = group.Sum(item => item.Value) })
            .ToListAsync(cancellationToken);
        return totals.ToDictionary(item => item.MetricCode, item => item.Value, StringComparer.Ordinal);
    }

    private async Task<double> GetLongestWorkoutStreakAsync(Guid userId, CancellationToken cancellationToken)
    {
        var occurredAt = await _context.ActivityContributions.AsNoTracking().Where(contribution => contribution.UserId == userId &&
                contribution.IsActive && contribution.MetricCode == MetricCatalogue.WorkoutCount)
            .Select(contribution => contribution.OccurredAt).ToListAsync(cancellationToken);
        if (occurredAt.Count == 0)
            return 0;

        var timeZoneId = await _context.UserPreferences.AsNoTracking().Where(preference => preference.UserId == userId)
            .Select(preference => preference.TimeZoneId).FirstOrDefaultAsync(cancellationToken) ?? DefaultTimeZoneId;
        return WorkoutStreakCalculator.GetLongestStreakDays(occurredAt, timeZoneId);
    }

    private async Task<double> GetPersonalRecordAchievementCountAsync(Guid userId, CancellationToken cancellationToken)
    {
        var history = await _context.PersonalRecordHistory.AsNoTracking().Where(record => record.UserId == userId)
            .Select(record => new PersonalRecordHistoryValue(record.WorkoutType, record.Metric, record.Value, record.RecordedAt, record.Id))
            .ToListAsync(cancellationToken);
        return CountRecordAchievements(history);
    }

    private async Task<ChallengeCounts> GetChallengeCountsAsync(Guid userId, CancellationToken cancellationToken)
    {
        var counts = await _context.ChallengeResults.AsNoTracking().Where(result => result.UserId == userId)
            .GroupBy(_ => 1)
            .Select(group => new ChallengeCounts(group.Count(), group.Count(result => result.IsFinisher), group.Count(result => result.IsWinner)))
            .FirstOrDefaultAsync(cancellationToken);
        return counts ?? new ChallengeCounts(0, 0, 0);
    }

    private static double GetTotal(IReadOnlyDictionary<string, double> totals, string metricCode) =>
        totals.TryGetValue(metricCode, out var value) ? value : 0;

    private static double CountRecordAchievements(IEnumerable<PersonalRecordHistoryValue> history)
    {
        var count = 0;
        foreach (var records in history.GroupBy(record => (record.WorkoutType, record.Metric)))
        {
            double? highest = null;
            foreach (var record in records.OrderBy(record => record.RecordedAt).ThenBy(record => record.Id))
            {
                if (highest.HasValue && record.Value <= highest.Value)
                    continue;

                highest = record.Value;
                count++;
            }
        }

        return count;
    }

    private sealed record PersonalRecordHistoryValue(string WorkoutType, string Metric, double Value, DateTime RecordedAt, Guid Id);
    private sealed record ChallengeCounts(int Participation, int TargetCompletions, int Wins);
}
