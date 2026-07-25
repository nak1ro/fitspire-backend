using backend.Modules.AiCoaching.Configuration;
using backend.Modules.AiCoaching.Contracts;
using backend.Modules.AiCoaching.Infrastructure;
using backend.Modules.AiCoaching.Services;
using FluentValidation;
using Microsoft.Extensions.Options;

namespace backend.Modules.AiCoaching;

public static class AiCoachingModuleExtensions
{
    public static IServiceCollection AddAiCoachingModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<OpenAiOptions>()
            .Bind(configuration.GetSection(OpenAiOptions.SectionName))
            .ValidateOnStart();
        services.AddSingleton<IValidateOptions<OpenAiOptions>, OpenAiOptionsValidator>();

        services.AddHttpClient<IGenerativeAiClient, OpenAiResponsesClient>((serviceProvider, client) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<OpenAiOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl, UriKind.Absolute);
            client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
        });
        services.AddScoped<IWeeklyCoachPeriodService, WeeklyCoachPeriodService>();
        services.AddScoped<IWeeklyCoachSnapshotBuilder, WeeklyCoachSnapshotBuilder>();
        services.AddScoped<IWeeklyCoachReportOutputValidator, WeeklyCoachReportOutputValidator>();
        services.AddScoped<IWeeklyCoachReportGenerationService, WeeklyCoachReportGenerationService>();
        services.AddScoped<IWeeklyCoachReportService, WeeklyCoachReportService>();
        services.AddScoped<IWeeklyCoachReportResponseFactory, WeeklyCoachReportResponseFactory>();
        services.AddScoped<IValidator<GenerateWeeklyCoachReportRequest>, GenerateWeeklyCoachReportRequestValidator>();
        services.AddScoped<IValidator<WeeklyCoachReportHistoryFilter>, WeeklyCoachReportHistoryFilterValidator>();
        services.AddHostedService<WeeklyCoachReportGenerationHostedService>();

        return services;
    }
}
