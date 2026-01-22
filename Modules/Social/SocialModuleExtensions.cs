using backend.Modules.Social.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace backend.Modules.Social;

public static class SocialModuleExtensions
{
    public static IServiceCollection AddSocialModule(this IServiceCollection services)
    {
        services.AddScoped<ISocialRepository, SocialRepository>();
        return services;
    }
}
