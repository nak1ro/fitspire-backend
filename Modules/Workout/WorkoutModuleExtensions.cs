using backend.Modules.Workout.DTOs;
using backend.Modules.Workout.Infrastructure;
using backend.Modules.Workout.Validators;
using backend.Modules.Workout.Services;
using FluentValidation;

namespace backend.Modules.Workout;

public static class WorkoutModuleExtensions
{
    public static IServiceCollection AddWorkoutModule(this IServiceCollection services)
    {
        // Repository
        services.AddScoped<IWorkoutRepository, WorkoutRepository>();
        services.AddScoped<IPersonalRecordRecalculationService, PersonalRecordRecalculationService>();
        services.AddScoped<IValidator<CreateGymWorkoutRequest>, CreateGymWorkoutValidator>();
        services.AddScoped<IValidator<CreateRunningWorkoutRequest>, CreateRunningWorkoutValidator>();
        services.AddScoped<IValidator<CreateCyclingWorkoutRequest>, CreateCyclingWorkoutValidator>();
        services.AddScoped<IValidator<CreateSwimmingWorkoutRequest>, CreateSwimmingWorkoutValidator>();
        services.AddScoped<IValidator<CreateYogaWorkoutRequest>, CreateYogaWorkoutValidator>();
        services.AddScoped<IValidator<CompleteWorkoutRequest>, CompleteWorkoutValidator>();
        services.AddScoped<IValidator<UpdateWorkoutRequest>, UpdateWorkoutValidator>();
        services.AddScoped<IValidator<WorkoutFilterRequest>, WorkoutFilterValidator>();
        services.AddScoped<IValidator<SaveRoutineRequest>, SaveRoutineValidator>();
        services.AddScoped<IValidator<CreateFromRoutineRequest>, CreateFromRoutineValidator>();
        services.AddScoped<IValidator<UpdateRoutineRequest>, UpdateRoutineValidator>();

        return services;
    }
}
