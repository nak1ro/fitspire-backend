using backend.Data;
using backend.Modules.Workout.Domain.Entities;
using backend.Modules.Workout.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace backend.Modules.Workout.Services;

public interface IPersonalRecordRecalculationService { Task RecalculateAsync(Guid userId, CancellationToken cancellationToken = default); }

public class PersonalRecordRecalculationService : IPersonalRecordRecalculationService
{
    private readonly FitspireDbContext _context; public PersonalRecordRecalculationService(FitspireDbContext context) => _context = context;
    public async Task RecalculateAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var workouts = await _context.UserWorkouts.Include(item => ((GymUserWorkoutDetails)item).Exercises)
            .Where(item => item.UserId == userId && item.Status == WorkoutStatus.Completed && item.DeletedAt == null)
            .ToListAsync(cancellationToken);
        var candidates = new Dictionary<(string Type, string Metric), (double Value, Guid WorkoutId)>();
        foreach (var workout in workouts)
        {
            Add(candidates, workout.WorkoutType, "duration", workout.DurationMinutes, workout.Id);
            Add(candidates, workout.WorkoutType, "calories", workout.CaloriesBurned, workout.Id);
            Add(candidates, workout.WorkoutType, "distance", workout.GetTotalDistance(), workout.Id);
            if (workout is GymUserWorkoutDetails gym)
            {
                Add(candidates, "gym", "max_weight", gym.GetMaxWeight(), workout.Id);
                Add(candidates, "gym", "total_volume", gym.CalculateTotalVolume(), workout.Id);
            }
        }

        var historicalMaximums = await GetHistoricalMaximumsAsync(userId, cancellationToken);
        var existing = await _context.PersonalRecords.Where(record => record.UserId == userId).ToListAsync(cancellationToken);
        foreach (var record in existing)
        {
            if (candidates.Remove((record.WorkoutType, record.Metric), out var candidate))
            {
                if (record.Value != candidate.Value || record.WorkoutId != candidate.WorkoutId)
                {
                    if (candidate.Value > record.Value)
                        record.TryBeat(candidate.Value, candidate.WorkoutId);
                    else
                        record.Replace(candidate.Value, candidate.WorkoutId);

                    await AddAchievementHistoryIfNewAsync(record, historicalMaximums, cancellationToken);
                }
            }
            else
            {
                _context.PersonalRecords.Remove(record);
            }
        }

        foreach (var ((type, metric), candidate) in candidates)
        {
            var record = PersonalRecord.Create(userId, type, metric, candidate.Value, candidate.WorkoutId);
            await _context.PersonalRecords.AddAsync(record, cancellationToken);
            await AddAchievementHistoryIfNewAsync(record, historicalMaximums, cancellationToken);
        }
    }

    private static void Add(Dictionary<(string, string), (double, Guid)> values, string type, string metric, double? value, Guid workoutId)
    {
        if (value is not > 0)
            return;

        var key = (type, metric);
        if (!values.TryGetValue(key, out var current) || value > current.Item1)
            values[key] = (value.Value, workoutId);
    }

    private async Task<Dictionary<(string Type, string Metric), double>> GetHistoricalMaximumsAsync(Guid userId,
        CancellationToken cancellationToken)
    {
        var values = await _context.PersonalRecordHistory.Where(history => history.UserId == userId)
            .GroupBy(history => new { history.WorkoutType, history.Metric })
            .Select(group => new { group.Key.WorkoutType, group.Key.Metric, Value = group.Max(history => history.Value) })
            .ToListAsync(cancellationToken);
        return values.ToDictionary(value => (value.WorkoutType, value.Metric), value => value.Value);
    }

    private async Task AddAchievementHistoryIfNewAsync(PersonalRecord record,
        IDictionary<(string Type, string Metric), double> historicalMaximums, CancellationToken cancellationToken)
    {
        var key = (record.WorkoutType, record.Metric);
        if (historicalMaximums.TryGetValue(key, out var highestValue) && record.Value <= highestValue)
            return;

        await _context.PersonalRecordHistory.AddAsync(new PersonalRecordHistory
        {
            Id = Guid.NewGuid(), UserId = record.UserId, WorkoutType = record.WorkoutType, Metric = record.Metric,
            Value = record.Value, WorkoutId = record.WorkoutId, RecordedAt = DateTime.UtcNow
        }, cancellationToken);
        historicalMaximums[key] = record.Value;
    }
}
