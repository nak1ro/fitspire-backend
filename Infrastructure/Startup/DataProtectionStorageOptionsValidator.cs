using Microsoft.Extensions.Options;

namespace backend.Infrastructure.Startup;

public sealed class DataProtectionStorageOptionsValidator(IHostEnvironment environment)
    : IValidateOptions<DataProtectionStorageOptions>
{
    public ValidateOptionsResult Validate(string? name, DataProtectionStorageOptions options)
    {
        if (!environment.IsProduction())
            return ValidateOptionsResult.Success;

        var errors = new List<string>();
        if (!IsHttpsServiceUri(options.ServiceUri))
            errors.Add("DataProtection:ServiceUri must be an absolute HTTPS Azure Blob service URI without a query string.");

        if (!IsValidContainerName(options.ContainerName))
            errors.Add("DataProtection:ContainerName must be a valid Azure Blob container name.");

        return errors.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(errors);
    }

    private static bool IsHttpsServiceUri(string value) =>
        Uri.TryCreate(value, UriKind.Absolute, out var uri)
        && uri.Scheme == Uri.UriSchemeHttps
        && string.IsNullOrEmpty(uri.Query)
        && string.IsNullOrEmpty(uri.Fragment)
        && (uri.AbsolutePath == "/" || string.IsNullOrEmpty(uri.AbsolutePath));

    private static bool IsValidContainerName(string value)
    {
        if (value.Length is < 3 or > 63)
            return false;

        return value.All(character => char.IsLower(character) || char.IsDigit(character) || character == '-')
               && char.IsLetterOrDigit(value[0])
               && char.IsLetterOrDigit(value[^1])
               && !value.Contains("--", StringComparison.Ordinal);
    }
}
