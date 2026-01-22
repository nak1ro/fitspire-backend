using backend.Data;
using backend.Modules.Goal.Domain.Entities;
using backend.Modules.Goal.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace backend.Modules.Goal.Infrastructure;

public class GoalRepository : IGoalRepository
{
    private readonly FitspireDbContext _context;

    public GoalRepository(FitspireDbContext context)
    {
        _context = context;
    }

    public async Task<UserGoal?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Goals
            .Include(g => g.GoalType)
            .FirstOrDefaultAsync(g => g.Id == id, cancellationToken);
    }

    public async Task<List<UserGoal>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Goals
            .Include(g => g.GoalType)
            .Where(g => g.UserId == userId)
            .OrderByDescending(g => g.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<UserGoal>> GetActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Goals
            .Include(g => g.GoalType)
            .Where(g => g.UserId == userId && g.Status == GoalStatus.Active)
            .OrderByDescending(g => g.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(UserGoal goal, CancellationToken cancellationToken = default)
    {
        await _context.Goals.AddAsync(goal, cancellationToken);
    }

    public Task UpdateAsync(UserGoal goal, CancellationToken cancellationToken = default)
    {
        _context.Goals.Update(goal);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(UserGoal goal, CancellationToken cancellationToken = default)
    {
        _context.Goals.Remove(goal);
        return Task.CompletedTask;
    }

    // GoalTypes
    public async Task<List<GoalType>> GetAllGoalTypesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.GoalTypes.ToListAsync(cancellationToken);
    }

    public async Task<GoalType?> GetGoalTypeByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.GoalTypes.FindAsync(new object[] { id }, cancellationToken);
    }

    // Progress
    public async Task AddProgressEntryAsync(GoalProgressEntry entry, CancellationToken cancellationToken = default)
    {
        await _context.GoalProgressEntries.AddAsync(entry, cancellationToken);
    }

    public async Task<List<GoalProgressEntry>> GetProgressHistoryAsync(Guid goalId, CancellationToken cancellationToken = default)
    {
        return await _context.GoalProgressEntries
            .Where(pe => pe.GoalId == goalId)
            .OrderByDescending(pe => pe.RecordedAt)
            .ToListAsync(cancellationToken);
    }

    // Cross-module queries
    public async Task<List<UserGoal>> GetActiveGoalsByWorkoutTypeAsync(Guid userId, string workoutType, CancellationToken cancellationToken = default)
    {
        return await _context.Goals
            .Include(g => g.GoalType)
            .Where(g => g.UserId == userId 
                        && g.Status == GoalStatus.Active 
                        && (g.GoalType.RelatedWorkoutType == workoutType || g.GoalType.RelatedWorkoutType == "any" || g.GoalType.RelatedWorkoutType == null))
            .ToListAsync(cancellationToken);
    }
}
