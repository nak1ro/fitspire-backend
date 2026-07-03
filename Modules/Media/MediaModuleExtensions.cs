using Azure.Identity;
using Azure.Storage.Blobs;
using backend.Modules.Media.Configuration;
using backend.Modules.Media.Contracts;
using backend.Modules.Media.Infrastructure;
using backend.Modules.Media.Services;
using FluentValidation;
using Microsoft.Extensions.Options;

namespace backend.Modules.Media;

public static class MediaModuleExtensions
{
    public static IServiceCollection AddMediaModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<MediaStorageOptions>()
            .Bind(configuration.GetSection(MediaStorageOptions.SectionName))
            .ValidateOnStart();
        services.AddSingleton<IValidateOptions<MediaStorageOptions>, MediaStorageOptionsValidator>();

        services.AddSingleton(serviceProvider =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<MediaStorageOptions>>().Value;
            return options.UsesConnectionString
                ? new BlobServiceClient(options.ConnectionString)
                : new BlobServiceClient(new Uri(options.ServiceUrl!), new DefaultAzureCredential());
        });

        services.AddSingleton<AzureMediaObjectStorage>();
        services.AddSingleton<IMediaObjectStorage>(serviceProvider => serviceProvider.GetRequiredService<AzureMediaObjectStorage>());
        services.AddHostedService<MediaStorageInitializer>();
        services.AddHostedService<MediaCleanupHostedService>();
        services.AddScoped<IImageProcessor, ImageSharpImageProcessor>();
        services.AddScoped<IMediaUploadService, MediaUploadService>();
        services.AddScoped<IMediaResponseFactory, MediaResponseFactory>();
        services.AddScoped<MediaCleanupService>();
        services.AddScoped<IValidator<InitiateMediaUploadRequest>, InitiateMediaUploadRequestValidator>();

        return services;
    }
}
