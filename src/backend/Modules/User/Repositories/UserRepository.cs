using backend.Data;
using backend.Modules.User.Domain;
using Microsoft.EntityFrameworkCore;

namespace backend.Modules.User.Repositories;

public class UserRepository : IUserRepository
{
    private readonly FitspireDbContext _db;

    public UserRepository(FitspireDbContext db)
    {
        _db = db;
    }

    public Task<AppUser?> GetByIdWithPrefsAsync(Guid userId)
    {
        return _db.Users
            .Include(u => u.AppUserPreference)
            .FirstOrDefaultAsync(u => u.Id == userId);
    }

    public Task<AppUserPreference?> GetPreferencesAsync(Guid userId)
    {
        return _db.UserPreferences
            .FirstOrDefaultAsync(p => p.UserId == userId);
    }
    
    public async Task<AppUserPreference> EnsurePreferencesAsync(Guid userId)
    {
        var prefs = await GetPreferencesAsync(userId);
        if (prefs != null) return prefs;

        prefs = new AppUserPreference
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            PreferredLanguage = "en",
            IsDarkModeEnabled = false,
            ReceiveEmailNotifications = true,
            UnitSystem = "metric",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.UserPreferences.Add(prefs);
        await _db.SaveChangesAsync();
        return prefs;
    }
    
    public async Task<AppUserPreference> UpsertPreferencesAsync(Guid userId, Action<AppUserPreference> applyChanges)
    {
        var prefs = await GetPreferencesAsync(userId);
        if (prefs == null)
        {
            prefs = new AppUserPreference
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                PreferredLanguage = "en",
                IsDarkModeEnabled = false,
                ReceiveEmailNotifications = true,
                UnitSystem = "metric",
                CreatedAt = DateTime.UtcNow
            };
            _db.UserPreferences.Add(prefs);
        }

        applyChanges(prefs);
        prefs.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return prefs;
    }

    public Task<AppUser?> GetByUsernameAsync(string username)
    {
        return _db.Users
            .Include(u => u.AppUserPreference)
            .FirstOrDefaultAsync(u => u.UserName == username);
    }
}