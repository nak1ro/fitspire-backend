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
        var workouts = await _context.UserWorkouts.Include(item => ((GymUserWorkoutDetails)item).Exercises).Where(item => item.UserId == userId && item.Status == WorkoutStatus.Completed && item.DeletedAt == null).ToListAsync(cancellationToken);
        var candidates = new Dictionary<(string Type, string Metric), (double Value, Guid WorkoutId)>();
        foreach (var workout in workouts)
        {
            Add(candidates, workout.WorkoutType, "duration", workout.DurationMinutes, workout.Id); Add(candidates, workout.WorkoutType, "calories", workout.CaloriesBurned, workout.Id); Add(candidates, workout.WorkoutType, "distance", workout.GetTotalDistance(), workout.Id);
            if (workout is GymUserWorkoutDetails gym) { Add(candidates, "gym", "max_weight", gym.GetMaxWeight(), workout.Id); Add(candidates, "gym", "total_volume", gym.CalculateTotalVolume(), workout.Id); }
        }
        var existing = await _context.PersonalRecords.Where(record => record.UserId == userId).ToListAsync(cancellationToken);
        foreach (var record in existing)
        {
            if (candidates.Remove((record.WorkoutType, record.Metric), out var candidate)) record.Replace(candidate.Value, candidate.WorkoutId);
            else _context.PersonalRecords.Remove(record);
        }
        foreach (var ((type, metric), candidate) in candidates) await _context.PersonalRecords.AddAsync(PersonalRecord.Create(userId, type, metric, candidate.Value, candidate.WorkoutId), cancellationToken);
    }
    private static void Add(Dictionary<(string, string), (double, Guid)> values, string type, string metric, double? value, Guid workoutId)
    {
        if (value is not > 0) return; var key = (type, metric); if (!values.TryGetValue(key, out var current) || value > current.Item1) values[key] = (value.Value, workoutId);
    }
}
