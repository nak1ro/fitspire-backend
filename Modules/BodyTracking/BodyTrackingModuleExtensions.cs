using backend.Modules.BodyTracking.Contracts;
using backend.Modules.BodyTracking.Services;
using backend.Modules.BodyTracking.Validators;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace backend.Modules.BodyTracking;

public static class BodyTrackingModuleExtensions
{
    public static IServiceCollection AddBodyTrackingModule(this IServiceCollection services)
    {
        services.AddScoped<IBodyCheckInWriteService, BodyCheckInWriteService>();
        services.AddScoped<IBodyCheckInTimeZoneService, BodyCheckInTimeZoneService>();
        services.AddScoped<IValidator<CreateBodyCheckInRequest>, CreateBodyCheckInRequestValidator>();
        services.AddScoped<IValidator<UpdateBodyCheckInRequest>, UpdateBodyCheckInRequestValidator>();
        services.AddScoped<IValidator<BodyCheckInHistoryFilter>, BodyCheckInHistoryFilterValidator>();
        services.AddScoped<IValidator<BodyCheckInSummaryFilter>, BodyCheckInSummaryFilterValidator>();
        return services;
    }
}
