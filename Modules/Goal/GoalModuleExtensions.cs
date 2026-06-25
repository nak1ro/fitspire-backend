using backend.Modules.Goal.DTOs;
using backend.Modules.Goal.Infrastructure;
using backend.Modules.Goal.Validators;
using backend.Modules.Goal.Services;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace backend.Modules.Goal;

public static class GoalModuleExtensions
{
    public static IServiceCollection AddGoalModule(this IServiceCollection services)
    {
        services.AddScoped<IGoalRepository, Infrastructure.GoalRepository>();
        services.AddScoped<IValidator<CreateGoalRequest>, CreateGoalRequestValidator>();
        services.AddScoped<IValidator<UpdateGoalRequest>, UpdateGoalRequestValidator>();
        services.AddScoped<IValidator<GoalListFilter>, GoalListFilterValidator>();
        services.AddScoped<IValidator<GoalPagination>, GoalPaginationValidator>();
        services.AddScoped<IGoalProgressService, GoalProgressService>();
        services.AddScoped<IGoalTemplatePolicy, GoalTemplatePolicy>();
        services.AddScoped<IGoalTransactionService, GoalTransactionService>();
        services.AddHostedService<GamificationLifecycleHostedService>();
        return services;
    }
}
