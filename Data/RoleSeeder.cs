using backend.Infrastructure.Startup;
using backend.Modules.Auth.Authorization;
using backend.Modules.User.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace backend.Data;

public static class RoleSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<AppUser>>();
        var options = serviceProvider.GetRequiredService<IOptions<AdministrationOptions>>().Value;
        var logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(RoleSeeder));

        string[] roles = [AppRoles.User, AppRoles.Admin];

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                var result = await roleManager.CreateAsync(new IdentityRole<Guid>(role));
                if (!result.Succeeded)
                    throw new InvalidOperationException($"Unable to seed role '{role}': {string.Join("; ", result.Errors.Select(error => error.Description))}");
            }
        }

        foreach (var email in options.InitialAdminEmails
                     .Select(value => value.Trim())
                     .Distinct(StringComparer.OrdinalIgnoreCase))
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user is null)
            {
                logger.LogWarning("Configured bootstrap administrator email {Email} does not match an existing account.", email);
                continue;
            }

            if (await userManager.IsInRoleAsync(user, AppRoles.Admin))
                continue;

            var result = await userManager.AddToRoleAsync(user, AppRoles.Admin);
            if (!result.Succeeded)
                throw new InvalidOperationException($"Unable to grant the administrator role: {string.Join("; ", result.Errors.Select(error => error.Description))}");
        }
    }
}
