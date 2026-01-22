namespace backend.Modules.Workout;

public static class WorkoutModuleExtensions
{
    public static IServiceCollection AddWorkoutModule(this IServiceCollection services)
    {
        // Repository
        services.AddScoped<IWorkoutRepository, WorkoutRepository>();

        return services;
    }
}
