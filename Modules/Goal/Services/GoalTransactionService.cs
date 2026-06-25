using backend.Data;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace backend.Modules.Goal.Services;

public interface IGoalTransactionService
{
    Task<T> ExecuteAsync<T>(Func<CancellationToken, Task<T>> action, CancellationToken cancellationToken = default);
}

public class GoalTransactionService : IGoalTransactionService
{
    private readonly FitspireDbContext _context;

    public GoalTransactionService(FitspireDbContext context) => _context = context;

    public async Task<T> ExecuteAsync<T>(Func<CancellationToken, Task<T>> action, CancellationToken cancellationToken = default)
    {
        if (_context.Database.CurrentTransaction is not null)
            return await action(cancellationToken);

        await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        var result = await action(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return result;
    }
}
