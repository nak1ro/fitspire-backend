using backend.Data;
using Microsoft.EntityFrameworkCore;

namespace backend.Modules.Badge.Services;

public interface IBadgeUserLockService
{
    Task AcquireAsync(Guid userId, CancellationToken cancellationToken = default);
}

public class BadgeUserLockService : IBadgeUserLockService
{
    private readonly FitspireDbContext _context;

    public BadgeUserLockService(FitspireDbContext context)
    {
        _context = context;
    }

    public Task AcquireAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var lockKey = BitConverter.ToInt64(userId.ToByteArray(), 0);
        return _context.Database.ExecuteSqlInterpolatedAsync($"SELECT pg_advisory_xact_lock({lockKey})", cancellationToken);
    }
}
