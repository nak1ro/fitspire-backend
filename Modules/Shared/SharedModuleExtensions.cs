using backend.Modules.Shared.Service;
using Resend;

namespace backend.Modules.Shared;

public static class SharedModuleExtensions
{
    public static IServiceCollection AddSharedModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpClient();
        services.AddHttpContextAccessor();
        services.Configure<ResendClientOptions>(configuration.GetSection("Resend"));
        services.AddScoped<ResendClient>();
        // UnitOfWork for all modules
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IUserLocalDateResolver, UserLocalDateResolver>();
        
        services.AddAutoMapper(_ => { }, AppDomain.CurrentDomain.GetAssemblies());

        return services;
    }
}
