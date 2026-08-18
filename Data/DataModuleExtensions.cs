using backend.Modules.User.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace backend.Data;

public static class DataModuleExtensions
{
    public static IServiceCollection AddDataModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<FitspireDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddIdentity<AppUser, IdentityRole<Guid>>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 6;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
            })
            .AddEntityFrameworkStores<FitspireDbContext>()
            .AddDefaultTokenProviders()
            .AddRoles<IdentityRole<Guid>>();

        return services;
    }
}
