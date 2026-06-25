using System.Data;
using backend.Data;
using Microsoft.EntityFrameworkCore;

namespace backend.Modules.Badge.Services;

public interface IBadgeTransactionService
{
    Task ExecuteAsync(Func<CancellationToken, Task> action, CancellationToken cancellationToken = default);
}

public class BadgeTransactionService : IBadgeTransactionService
{
    private readonly FitspireDbContext _context;

    public BadgeTransactionService(FitspireDbContext context)
    {
        _context = context;
    }

    public async Task ExecuteAsync(Func<CancellationToken, Task> action, CancellationToken cancellationToken = default)
    {
        if (_context.Database.CurrentTransaction is not null)
        {
            await action(cancellationToken);
            return;
        }

        await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        await action(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }
}
