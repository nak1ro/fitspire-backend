using backend.Modules.DemoData.Services;

namespace backend.Modules.DemoData;

public static class DemoDataModuleExtensions
{
    public static IServiceCollection AddDemoDataModule(this IServiceCollection services)
    {
        services.AddScoped<IDemoAccountService, DemoAccountService>();
        services.AddScoped<IDemoWorkoutService, DemoWorkoutService>();
        services.AddScoped<IDemoNutritionService, DemoNutritionService>();
        services.AddScoped<IDemoGoalService, DemoGoalService>();
        services.AddScoped<IDemoChallengeService, DemoChallengeService>();
        services.AddScoped<IDemoSocialService, DemoSocialService>();
        services.AddScoped<IDemoDataSeedingService, DemoDataSeedingService>();
        services.AddSingleton<IDemoDataSeedProgress, DemoDataSeedProgress>();
        return services;
    }
}
