using backend.Modules.Goal.Domain.Entities;
using backend.Modules.Workout.Domain.Entities;

namespace backend.Modules.Goal.Infrastructure;

public interface IGoalRepository
{
    Task<UserGoal?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<UserGoal>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<List<UserGoal>> GetActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task AddAsync(UserGoal goal, CancellationToken cancellationToken = default);
    Task UpdateAsync(UserGoal goal, CancellationToken cancellationToken = default);
    Task DeleteAsync(UserGoal goal, CancellationToken cancellationToken = default);
    
    // GoalTypes
    Task<List<GoalType>> GetAllGoalTypesAsync(CancellationToken cancellationToken = default);
    Task<GoalType?> GetGoalTypeByIdAsync(Guid id, CancellationToken cancellationToken = default);
    
    // Progress
    Task AddProgressEntryAsync(GoalProgressEntry entry, CancellationToken cancellationToken = default);
    Task<List<GoalProgressEntry>> GetProgressHistoryAsync(Guid goalId, CancellationToken cancellationToken = default);
    // Cross-module queries
    Task<List<UserGoal>> GetActiveGoalsByWorkoutTypeAsync(Guid userId, string workoutType, CancellationToken cancellationToken = default);
    Task<List<GoalProgressEntry>> GetProgressBySourceIdAsync(Guid sourceId, CancellationToken cancellationToken = default);
    Task RemoveProgressEntryAsync(GoalProgressEntry entry, CancellationToken cancellationToken = default);

    // Exercise-level queries
    Task<List<UserGoal>> GetActiveGoalsByExerciseIdAsync(Guid userId, Guid exerciseId, CancellationToken cancellationToken = default);
    Task<GymUserWorkoutDetails?> GetGymWorkoutByIdAsync(Guid workoutId, CancellationToken cancellationToken = default);
    Task<RunningUserWorkoutDetails?> GetRunningWorkoutByIdAsync(Guid workoutId, CancellationToken cancellationToken = default);
    Task<CyclingUserWorkoutDetails?> GetCyclingWorkoutByIdAsync(Guid workoutId, CancellationToken cancellationToken = default);
    Task<SwimmingUserWorkoutDetails?> GetSwimmingWorkoutByIdAsync(Guid workoutId, CancellationToken cancellationToken = default);
    Task<YogaUserWorkoutDetails?> GetYogaWorkoutByIdAsync(Guid workoutId, CancellationToken cancellationToken = default);
}
