using backend.Data;
using backend.Modules.Workout.Domain.Entities;
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
        return await _context.UserWorkouts.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<GymUserWorkoutDetails?> GetGymWorkoutByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Set<GymUserWorkoutDetails>()
            .Include(w => w.Exercises)
                .ThenInclude(e => e.Exercise)
            .FirstOrDefaultAsync(w => w.Id == id, cancellationToken);
    }

    public async Task<List<UserWorkout>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.UserWorkouts
            .Where(w => w.UserId == userId)
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
            .Where(w => w.UserId == userId);

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
}
