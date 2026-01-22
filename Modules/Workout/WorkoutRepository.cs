using backend.Data;
using backend.Modules.Workout.Domain;
using Microsoft.EntityFrameworkCore;

namespace backend.Modules.Workout;

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

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var workout = await GetByIdAsync(id, cancellationToken);
        if (workout is not null)
            _context.UserWorkouts.Remove(workout);
    }
}
