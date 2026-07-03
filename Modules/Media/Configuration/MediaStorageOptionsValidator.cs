using Microsoft.Extensions.Options;

namespace backend.Modules.Media.Configuration;

public class MediaStorageOptionsValidator : IValidateOptions<MediaStorageOptions>
{
    public ValidateOptionsResult Validate(string? name, MediaStorageOptions options)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(options.ContainerName))
            errors.Add("MediaStorage:ContainerName is required.");

        if (!options.UsesConnectionString && !Uri.TryCreate(options.ServiceUrl, UriKind.Absolute, out _))
            errors.Add("Media storage requires either a connection string or an absolute service URL.");

        if (options.UsesConnectionString && !string.IsNullOrWhiteSpace(options.ServiceUrl))
            errors.Add("Media storage must use either a connection string or a service URL, not both.");

        ValidatePositive(options.UploadSasMinutes, nameof(options.UploadSasMinutes), errors);
        ValidatePositive(options.ReadSasMinutes, nameof(options.ReadSasMinutes), errors);
        ValidatePositive(options.PendingUploadLifetimeMinutes, nameof(options.PendingUploadLifetimeMinutes), errors);
        ValidatePositive(options.CleanupIntervalMinutes, nameof(options.CleanupIntervalMinutes), errors);
        ValidatePositive(options.ProcessingTimeoutMinutes, nameof(options.ProcessingTimeoutMinutes), errors);
        ValidatePositive(options.UnattachedReadyLifetimeMinutes, nameof(options.UnattachedReadyLifetimeMinutes), errors);
        ValidatePositive(options.MaxPendingUploadsPerUser, nameof(options.MaxPendingUploadsPerUser), errors);
        ValidatePositive(options.ProfilePictureMaxBytes, nameof(options.ProfilePictureMaxBytes), errors);
        ValidatePositive(options.PostImageMaxBytes, nameof(options.PostImageMaxBytes), errors);
        ValidatePositive(options.MaxDecodedPixels, nameof(options.MaxDecodedPixels), errors);

        if (options.PostImageLimit is < 1 or > 10)
            errors.Add("MediaStorage:PostImageLimit must be between 1 and 10.");

        if (options.CleanupBatchSize is < 1 or > 200)
            errors.Add("MediaStorage:CleanupBatchSize must be between 1 and 200.");

        return errors.Count == 0 ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail(errors);
    }

    private static void ValidatePositive(long value, string propertyName, List<string> errors)
    {
        if (value <= 0)
            errors.Add($"MediaStorage:{propertyName} must be positive.");
    }
}
