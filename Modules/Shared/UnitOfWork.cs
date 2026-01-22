using backend.Data;

namespace backend.Modules.Shared;

public class UnitOfWork : IUnitOfWork
{
    private readonly FitspireDbContext _context;

    public UnitOfWork(FitspireDbContext context)
    {
        _context = context;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}
