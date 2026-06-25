using backend.Data;
using backend.Modules.Workout.Domain.Entities;
using backend.Modules.Workout.Domain.Enums;
using backend.Modules.Workout.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace backend.Modules.Workout.Infrastructure;

public class WorkoutRepository : IWorkoutRepository
{
    private readonly FitspireDbContext _context;

    public WorkoutRepository(FitspireDbContext context)
    {
        _context = context;
    }

    public async Task<UserWorkout?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.UserWorkouts
            .FirstOrDefaultAsync(workout => workout.Id == id && workout.DeletedAt == null, cancellationToken);
    }

    public async Task<UserWorkout?> GetArchivedByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.UserWorkouts
            .Include(workout => ((GymUserWorkoutDetails)workout).Exercises)
            .FirstOrDefaultAsync(workout => workout.Id == id && workout.DeletedAt != null, cancellationToken);
    }

    public async Task<UserWorkout?> GetDetailsByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Set<UserWorkout>()
            .Include(w => ((GymUserWorkoutDetails)w).Exercises)
                .ThenInclude(e => e.Exercise)
            .FirstOrDefaultAsync(w => w.Id == id && w.DeletedAt == null, cancellationToken);
    }

    public async Task<UserWorkout?> GetActiveSessionByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.UserWorkouts
            .Where(workout => workout.UserId == userId
                              && workout.DeletedAt == null
                              && (workout.Status == WorkoutStatus.InProgress
                                  || workout.Status == WorkoutStatus.Paused))
            .OrderByDescending(workout => workout.StartedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<UserWorkout>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
    {
        var workoutIds = ids.Distinct().ToList();

        return await _context.UserWorkouts
            .Include(w => ((GymUserWorkoutDetails)w).Exercises)
            .Where(w => workoutIds.Contains(w.Id))
            .Where(w => w.DeletedAt == null)
            .ToListAsync(cancellationToken);
    }

    public async Task<GymUserWorkoutDetails?> GetGymWorkoutByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Set<GymUserWorkoutDetails>()
            .Include(w => w.Exercises)
                .ThenInclude(e => e.Exercise)
            .FirstOrDefaultAsync(w => w.Id == id && w.DeletedAt == null, cancellationToken);
    }

    public async Task<List<UserWorkout>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.UserWorkouts
            .Where(w => w.UserId == userId && w.DeletedAt == null)
            .OrderByDescending(w => w.Date)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(UserWorkout workout, CancellationToken cancellationToken = default)
    {
        await _context.UserWorkouts.AddAsync(workout, cancellationToken);
    }

    public Task UpdateAsync(UserWorkout workout, CancellationToken cancellationToken = default)
    {
        _context.UserWorkouts.Update(workout);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(UserWorkout workout, CancellationToken cancellationToken = default)
    {
        _context.Remove(workout);
        return Task.CompletedTask;
    }

    public async Task<List<UserWorkout>> SearchAsync(Guid userId, DateTime? from, DateTime? to, List<string>? types, CancellationToken cancellationToken = default)
    {
        var query = _context.Set<UserWorkout>()
            // Optimistically include Gym Exercises if it's a gym workout
            .Include(w => ((GymUserWorkoutDetails)w).Exercises)
                .ThenInclude(e => e.Exercise)
            .Where(w => w.UserId == userId && w.DeletedAt == null);

        if (from.HasValue)
            query = query.Where(w => w.Date >= from.Value);

        if (to.HasValue)
            query = query.Where(w => w.Date <= to.Value);

        if (types != null && types.Any())
            query = query.Where(w => types.Contains(w.WorkoutType));

        return await query
            .OrderByDescending(w => w.Date)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<UserWorkout>> GetArchivedByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.UserWorkouts
            .Where(workout => workout.UserId == userId && workout.DeletedAt != null)
            .OrderByDescending(workout => workout.DeletedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<ExerciseCategory>> GetExerciseCategoriesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.ExerciseCategories
            .Include(c => c.Exercises)
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Exercise>> GetExercisesAsync(Guid? categoryId, string? search, CancellationToken cancellationToken = default)
    {
        var query = _context.Exercises
            .Include(e => e.Category)
            .AsQueryable();

        if (categoryId.HasValue)
            query = query.Where(e => e.CategoryId == categoryId.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalizedSearch = search.Trim().ToLower();
            query = query.Where(e => e.Name.ToLower().Contains(normalizedSearch));
        }

        return await query
            .OrderBy(e => e.Category != null ? e.Category.Name : string.Empty)
            .ThenBy(e => e.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task AddRoutineAsync(WorkoutRoutine routine, CancellationToken cancellationToken = default)
    {
        await _context.WorkoutRoutines.AddAsync(routine, cancellationToken);
    }

    public async Task<WorkoutRoutine?> GetRoutineByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.WorkoutRoutines.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<List<WorkoutRoutine>> GetRoutinesByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.WorkoutRoutines
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public Task DeleteRoutineAsync(WorkoutRoutine routine, CancellationToken cancellationToken = default)
    {
        _context.WorkoutRoutines.Remove(routine);
        return Task.CompletedTask;
    }

    public async Task<List<PersonalRecord>> GetPersonalRecordsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.PersonalRecords
            .Where(record => record.UserId == userId)
            .OrderBy(record => record.WorkoutType)
            .ThenBy(record => record.Metric)
            .ToListAsync(cancellationToken);
    }

    public async Task<PersonalRecord?> GetPersonalRecordAsync(
        Guid userId,
        string workoutType,
        string metric,
        CancellationToken cancellationToken = default)
    {
        return await _context.PersonalRecords
            .FirstOrDefaultAsync(
                record => record.UserId == userId
                    && record.WorkoutType == workoutType
                    && record.Metric == metric,
                cancellationToken);
    }

    public async Task AddPersonalRecordAsync(PersonalRecord record, CancellationToken cancellationToken = default)
    {
        await _context.PersonalRecords.AddAsync(record, cancellationToken);
    }

    public Task UpdatePersonalRecordAsync(PersonalRecord record, CancellationToken cancellationToken = default)
    {
        _context.PersonalRecords.Update(record);
        return Task.CompletedTask;
    }

    public async Task AddPersonalRecordHistoryAsync(PersonalRecordHistory history, CancellationToken cancellationToken = default)
    {
        await _context.PersonalRecordHistory.AddAsync(history, cancellationToken);
    }
}
