using backend.Modules.Goal.Domain.Entities;

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
}
