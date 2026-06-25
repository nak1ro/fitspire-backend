using backend.Modules.Progress.Services;

namespace backend.Modules.Progress;

public static class ProgressModuleExtensions
{
    public static IServiceCollection AddProgressModule(this IServiceCollection services)
    {
        services.AddScoped<IContributionReconciliationService, ContributionReconciliationService>();
        return services;
    }
}
