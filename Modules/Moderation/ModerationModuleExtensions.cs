using backend.Modules.Moderation.Contracts;
using backend.Modules.Moderation.Services;
using FluentValidation;

namespace backend.Modules.Moderation;

public static class ModerationModuleExtensions
{
    public static IServiceCollection AddModerationModule(this IServiceCollection services)
    {
        services.AddScoped<IModerationTargetResolver, ModerationTargetResolver>();
        services.AddScoped<IModerationReportService, ModerationReportService>();
        services.AddScoped<IAdminModerationService, AdminModerationService>();
        services.AddScoped<IModerationMediaPreviewService, ModerationMediaPreviewService>();
        services.AddScoped<IValidator<CreateModerationReportRequest>, CreateModerationReportRequestValidator>();
        services.AddScoped<IValidator<AdminModerationReportFilter>, AdminModerationReportFilterValidator>();
        services.AddScoped<IValidator<ResolveModerationReportRequest>, ResolveModerationReportRequestValidator>();

        return services;
    }
}
