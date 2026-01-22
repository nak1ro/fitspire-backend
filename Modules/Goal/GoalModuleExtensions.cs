using backend.Modules.Goal.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace backend.Modules.Goal;

public static class GoalModuleExtensions
{
    public static IServiceCollection AddGoalModule(this IServiceCollection services)
    {
        services.AddScoped<IGoalRepository, GoalRepository>();
        return services;
    }
}
