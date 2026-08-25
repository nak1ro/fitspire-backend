using backend.Modules.Auth.Authorization;
using backend.Modules.DemoData.Domain;
using backend.Modules.Shared.Domain;
using backend.Modules.User.Domain;
using Microsoft.AspNetCore.Identity;

namespace backend.Modules.DemoData.Services;

public interface IDemoAccountService
{
    Task<Guid> CreateHeroAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<Guid>> CreateFillersAsync(CancellationToken cancellationToken);
}

public class DemoAccountService : IDemoAccountService
{
    private readonly UserManager<AppUser> _userManager;

    public DemoAccountService(UserManager<AppUser> userManager) => _userManager = userManager;

    public async Task<Guid> CreateHeroAsync(CancellationToken cancellationToken)
    {
        var user = new AppUser
        {
            UserName = DemoDataConstants.HeroUserName,
            Email = DemoDataConstants.HeroEmail,
            DisplayName = DemoDataConstants.HeroDisplayName,
            Bio = DemoDataConstants.HeroBio,
            EmailConfirmed = true,
            IsPrivate = false,
        };
        return await GetOrCreateAsync(user);
    }

    public async Task<IReadOnlyList<Guid>> CreateFillersAsync(CancellationToken cancellationToken)
    {
        var ids = new List<Guid>();
        foreach (var (displayName, userName, bio) in DemoDataConstants.FillerAccounts)
        {
            var user = new AppUser
            {
                UserName = userName,
                Email = $"{userName}@fitspire.demo",
                DisplayName = displayName,
                Bio = bio,
                EmailConfirmed = true,
                IsPrivate = false,
            };
            ids.Add(await GetOrCreateAsync(user));
        }
        return ids;
    }

    // Idempotent so a retry after a crash midway through the wider seed pipeline doesn't fail on
    // "email already in use" for accounts a previous attempt already created successfully.
    private async Task<Guid> GetOrCreateAsync(AppUser user)
    {
        var existing = await _userManager.FindByEmailAsync(user.Email!);
        if (existing is not null)
            return existing.Id;

        var result = await _userManager.CreateAsync(user, DemoDataConstants.DemoPassword);
        if (!result.Succeeded)
            throw new DomainException($"Failed to create demo account {user.UserName}: " +
                string.Join("; ", result.Errors.Select(error => error.Description)));

        await _userManager.AddToRoleAsync(user, AppRoles.User);
        return user.Id;
    }
}
