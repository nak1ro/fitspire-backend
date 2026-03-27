using backend.Modules.User.Services;

namespace backend.Modules.User;

public static class UserModuleExtensions
{
    public static IServiceCollection AddUserModule(this IServiceCollection services)
    {
        services.AddScoped<IUserService, UserService>();
        return services;
    }
}
