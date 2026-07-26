using Azure.Identity;
using backend.Data;
using backend.Modules.Badge.Data;
using backend.Modules.Goal.Data;
using backend.Modules.Progress.Data;
using backend.Modules.Workout.Data.Seeding;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Options;

namespace backend.Infrastructure.Startup;

public static class StartupExtensions
{
    public static IServiceCollection AddFitspireDataProtection(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        var validator = new DataProtectionStorageOptionsValidator(environment);
        services.AddOptions<DataProtectionStorageOptions>()
            .Bind(configuration.GetSection(DataProtectionStorageOptions.SectionName))
            .ValidateOnStart();
        services.AddSingleton<IValidateOptions<DataProtectionStorageOptions>>(validator);

        var dataProtection = services.AddDataProtection()
            .SetApplicationName(DataProtectionStorageOptions.ApplicationName);

        if (environment.IsProduction())
            ConfigureProductionKeyStorage(dataProtection, configuration, validator);

        return services;
    }

    public static IServiceCollection AddStartupInitialization(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<StartupOptions>()
            .Bind(configuration.GetSection(StartupOptions.SectionName))
            .ValidateOnStart();

        return services;
    }

    public static async Task InitializeStartupAsync(this WebApplication app, CancellationToken cancellationToken = default)
    {
        var options = app.Services.GetRequiredService<IOptions<StartupOptions>>().Value;
        await using var scope = app.Services.CreateAsyncScope();
        var serviceProvider = scope.ServiceProvider;
        var context = serviceProvider.GetRequiredService<FitspireDbContext>();

        if (options.ApplyMigrationsOnStartup)
            await ApplyMigrationsAsync(app.Logger, context, cancellationToken);
        else
            app.Logger.LogInformation("Database migration on startup is disabled.");

        await SeedAsync(app.Logger, serviceProvider, context, cancellationToken);
    }

    private static void ConfigureProductionKeyStorage(
        IDataProtectionBuilder dataProtection,
        IConfiguration configuration,
        DataProtectionStorageOptionsValidator validator)
    {
        var options = configuration.GetSection(DataProtectionStorageOptions.SectionName).Get<DataProtectionStorageOptions>()
                      ?? new DataProtectionStorageOptions();
        var validation = validator.Validate(null, options);
        if (validation.Failed)
            throw new InvalidOperationException(string.Join(" ", validation.Failures));

        var keyBlobUri = new Uri(new Uri(options.ServiceUri),
            $"{options.ContainerName}/{DataProtectionStorageOptions.KeyBlobName}");
        dataProtection.PersistKeysToAzureBlobStorage(keyBlobUri, new DefaultAzureCredential());
    }

    private static async Task ApplyMigrationsAsync(ILogger logger, FitspireDbContext context, CancellationToken cancellationToken)
    {
        logger.LogInformation("Applying pending database migrations.");
        await context.Database.MigrateAsync(cancellationToken);
        logger.LogInformation("Database migrations completed.");
    }

    private static async Task SeedAsync(
        ILogger logger,
        IServiceProvider serviceProvider,
        FitspireDbContext context,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Running idempotent application seeders.");
        await RoleSeeder.SeedAsync(serviceProvider);
        await ExerciseSeeder.SeedAsync(serviceProvider);
        await MetricDefinitionSeeder.SeedAsync(context, cancellationToken);
        await BadgeSeeder.SeedAsync(context, cancellationToken);
        await new GoalTypeSeeder(context).SeedAsync(cancellationToken);
        logger.LogInformation("Application seeders completed.");
    }
}
