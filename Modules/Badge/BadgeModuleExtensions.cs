using backend.Modules.Badge.Services;

namespace backend.Modules.Badge;

public static class BadgeModuleExtensions
{
    public static IServiceCollection AddBadgeModule(this IServiceCollection services)
    {
        services.AddScoped<IBadgeEvaluationService, BadgeEvaluationService>();
        return services;
    }
}
