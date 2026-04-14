using backend.Modules.Goal.DTOs;
using backend.Modules.Goal.Infrastructure;
using backend.Modules.Goal.Services.MetricCalculators;
using backend.Modules.Goal.Validators;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace backend.Modules.Goal;

public static class GoalModuleExtensions
{
    public static IServiceCollection AddGoalModule(this IServiceCollection services)
    {
        services.AddScoped<IGoalRepository, Infrastructure.GoalRepository>();
        services.AddScoped<IValidator<CreateGoalRequest>, CreateGoalRequestValidator>();
        services.AddScoped<IValidator<UpdateGoalProgressRequest>, UpdateGoalProgressRequestValidator>();
        
        // Register Metric Strategies
        services.AddScoped<IMetricCalculator, CountMetricCalculator>();
        services.AddScoped<IMetricCalculator, DistanceMetricCalculator>();
        services.AddScoped<IMetricCalculator, DurationMetricCalculator>();
        services.AddScoped<IMetricCalculator, VolumeMetricCalculator>();
        services.AddScoped<IMetricCalculator, CaloriesMetricCalculator>();
        
        // Register Exercise Metric Strategies
        services.AddScoped<IExerciseMetricCalculator, MaxWeightCalculator>();
        services.AddScoped<IExerciseMetricCalculator, ExerciseVolumeCalculator>();
        services.AddScoped<IExerciseMetricCalculator, ExerciseRepsCalculator>();
        services.AddScoped<IExerciseMetricCalculator, ExerciseRepsCalculator>();
        services.AddScoped<IExerciseMetricCalculator, ExerciseCountCalculator>();
        
        // Register Workout Goal Processors
        services.AddScoped<Services.GoalProcessors.IWorkoutGoalProcessor, Services.GoalProcessors.GymGoalProcessor>();
        services.AddScoped<Services.GoalProcessors.IWorkoutGoalProcessor, Services.GoalProcessors.RunningGoalProcessor>();
        services.AddScoped<Services.GoalProcessors.IWorkoutGoalProcessor, Services.GoalProcessors.CyclingGoalProcessor>();
        services.AddScoped<Services.GoalProcessors.IWorkoutGoalProcessor, Services.GoalProcessors.SwimmingGoalProcessor>();
        services.AddScoped<Services.GoalProcessors.IWorkoutGoalProcessor, Services.GoalProcessors.YogaGoalProcessor>();
        
        return services;
    }
}
