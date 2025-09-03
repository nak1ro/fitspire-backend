using backend.Modules.User.Domain;

namespace backend.Modules.User.Repositories;

public interface IUserRepository
{
    Task<AppUser?> GetByIdWithPrefsAsync(Guid userId);

    Task<AppUserPreference?> GetPreferencesAsync(Guid userId);
    Task<AppUserPreference> EnsurePreferencesAsync(Guid userId);
    Task<AppUserPreference> UpsertPreferencesAsync(Guid userId, Action<AppUserPreference> applyChanges);
    Task<AppUser?> GetByUsernameAsync(string username);
}