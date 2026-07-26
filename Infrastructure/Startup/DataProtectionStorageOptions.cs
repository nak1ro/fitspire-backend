namespace backend.Infrastructure.Startup;

public sealed class DataProtectionStorageOptions
{
    public const string SectionName = "DataProtection";
    public const string ApplicationName = "FitspireBackend";
    public const string KeyBlobName = "key-ring.xml";

    public string ServiceUri { get; init; } = string.Empty;
    public string ContainerName { get; init; } = string.Empty;
}
