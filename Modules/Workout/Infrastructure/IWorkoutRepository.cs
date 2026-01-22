using backend.Modules.Workout.Domain;

using backend.Modules.Workout.Domain.Entities;

namespace backend.Modules.Workout.Infrastructure;

public interface IWorkoutRepository
{
    Task<UserWorkout?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<GymUserWorkoutDetails?> GetGymWorkoutByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<UserWorkout>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task AddAsync(UserWorkout workout, CancellationToken cancellationToken = default);
    Task UpdateAsync(UserWorkout workout, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
