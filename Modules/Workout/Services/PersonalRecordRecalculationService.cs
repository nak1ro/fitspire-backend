using backend.Data;
using backend.Modules.Workout.Domain.Entities;
using backend.Modules.Workout.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace backend.Modules.Workout.Services;

public interface IPersonalRecordRecalculationService { Task RecalculateAsync(Guid userId, CancellationToken cancellationToken = default); }

public class PersonalRecordRecalculationService : IPersonalRecordRecalculationService
{
    private readonly FitspireDbContext _context;
    public PersonalRecordRecalculationService(FitspireDbContext context) => _context = context;

    public async Task RecalculateAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var workouts = await _context.UserWorkouts
            .Include(w => ((GymUserWorkoutDetails)w).Exercises).ThenInclude(e => e.WorkoutSets)
            .Where(w => w.UserId == userId && w.Status == WorkoutStatus.Completed && w.DeletedAt == null)
            .OrderBy(w => w.Date).ThenBy(w => w.Id).ToListAsync(cancellationToken);
        var candidates = BuildCandidates(workouts);
        var history = await GetHistoricalMaximumsAsync(userId, cancellationToken);
        var existing = await _context.PersonalRecords.Where(record => record.UserId == userId).ToListAsync(cancellationToken);

        foreach (var record in existing)
        {
            var key = new RecordKey(record.WorkoutType, record.Metric, record.ExerciseId);
            if (!candidates.Remove(key, out var candidate)) { _context.PersonalRecords.Remove(record); continue; }
            if (record.Value == candidate.Value && record.WorkoutId == candidate.WorkoutId) continue;
            if (candidate.Value > record.Value) record.TryBeat(candidate.Value, candidate.WorkoutId, candidate.OccurredAt);
            else record.Replace(candidate.Value, candidate.WorkoutId, candidate.OccurredAt);
            await AddHistoryIfNewAsync(record, candidate.OccurredAt, history, cancellationToken);
        }

        foreach (var (key, candidate) in candidates)
        {
            var record = PersonalRecord.Create(userId, key.WorkoutType, key.Metric, key.ExerciseId, candidate.Value, candidate.WorkoutId, candidate.OccurredAt);
            await _context.PersonalRecords.AddAsync(record, cancellationToken);
            await AddHistoryIfNewAsync(record, candidate.OccurredAt, history, cancellationToken);
        }
    }

    private static Dictionary<RecordKey, Candidate> BuildCandidates(IEnumerable<UserWorkout> workouts)
    {
        var candidates = new Dictionary<RecordKey, Candidate>();
        foreach (var workout in workouts)
        {
            Add(candidates, workout, PersonalRecordMetricCatalogue.DurationMinutes, workout.DurationMinutes);
            Add(candidates, workout, PersonalRecordMetricCatalogue.Calories, workout.CaloriesBurned);
            Add(candidates, workout, PersonalRecordMetricCatalogue.Distance, workout.GetTotalDistance());
            if (workout is GymUserWorkoutDetails gym) AddGymCandidates(candidates, gym);
        }
        return candidates;
    }

    private static void AddGymCandidates(IDictionary<RecordKey, Candidate> candidates, GymUserWorkoutDetails gym)
    {
        Add(candidates, gym, PersonalRecordMetricCatalogue.TotalVolume, gym.GetTotalVolume());
        Add(candidates, gym, PersonalRecordMetricCatalogue.MaximumWeight, gym.GetMaxWeight());
        foreach (var exercise in gym.Exercises)
        {
            var sets = exercise.WorkoutSets.Where(set => set.IsCompleted).ToList();
            Add(candidates, gym, PersonalRecordMetricCatalogue.MaximumWeight, exercise.GetMaximumCompletedWeight(), exercise.ExerciseId);
            Add(candidates, gym, PersonalRecordMetricCatalogue.TotalVolume, exercise.CalculateCompletedSetVolume(), exercise.ExerciseId);
            Add(candidates, gym, PersonalRecordMetricCatalogue.MaximumSetVolume, sets.Where(s => s.Reps.HasValue && s.WeightKg.HasValue).Select(s => (double?)(s.Reps!.Value * s.WeightKg!.Value)).Max(), exercise.ExerciseId);
            Add(candidates, gym, PersonalRecordMetricCatalogue.MaximumReps, sets.Select(s => s.Reps).Max(), exercise.ExerciseId);
            Add(candidates, gym, PersonalRecordMetricCatalogue.EstimatedOneRepMax, sets.Where(s => s.Reps.HasValue && s.WeightKg.HasValue).Select(s => (double?)(s.WeightKg!.Value * (1d + s.Reps!.Value / 30d))).Max(), exercise.ExerciseId);
        }
    }

    private static void Add(IDictionary<RecordKey, Candidate> candidates, UserWorkout workout, string metric, double? value, Guid? exerciseId = null)
    {
        if (value is not > 0) return;
        var key = new RecordKey(workout.WorkoutType, metric, exerciseId);
        var candidate = new Candidate(value.Value, workout.Id, workout.Date);
        if (!candidates.TryGetValue(key, out var current) || candidate.IsBetterThan(current)) candidates[key] = candidate;
    }

    private async Task<Dictionary<RecordKey, double>> GetHistoricalMaximumsAsync(Guid userId, CancellationToken cancellationToken)
    {
        var values = await _context.PersonalRecordHistory.Where(history => history.UserId == userId)
            .GroupBy(history => new { history.WorkoutType, history.Metric, history.ExerciseId })
            .Select(group => new { group.Key.WorkoutType, group.Key.Metric, group.Key.ExerciseId, Value = group.Max(history => history.Value) }).ToListAsync(cancellationToken);
        return values.ToDictionary(value => new RecordKey(value.WorkoutType, value.Metric, value.ExerciseId), value => value.Value);
    }

    private async Task AddHistoryIfNewAsync(PersonalRecord record, DateTime occurredAt, IDictionary<RecordKey, double> history, CancellationToken cancellationToken)
    {
        var key = new RecordKey(record.WorkoutType, record.Metric, record.ExerciseId);
        if (history.TryGetValue(key, out var highest) && record.Value <= highest) return;
        await _context.PersonalRecordHistory.AddAsync(new PersonalRecordHistory { Id = Guid.NewGuid(), UserId = record.UserId, WorkoutType = record.WorkoutType, Metric = record.Metric, ExerciseId = record.ExerciseId, Value = record.Value, WorkoutId = record.WorkoutId, RecordedAt = occurredAt }, cancellationToken);
        history[key] = record.Value;
    }

    private readonly record struct RecordKey(string WorkoutType, string Metric, Guid? ExerciseId);
    private readonly record struct Candidate(double Value, Guid WorkoutId, DateTime OccurredAt)
    {
        public bool IsBetterThan(Candidate other) => Value > other.Value || (Value == other.Value && OccurredAt < other.OccurredAt);
    }
}
