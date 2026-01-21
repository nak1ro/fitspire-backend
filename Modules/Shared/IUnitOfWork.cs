namespace backend.Modules.Shared;

/// <summary>
/// Abstraction for saving changes to the database.
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
