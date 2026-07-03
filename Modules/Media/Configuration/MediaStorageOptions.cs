namespace backend.Modules.Media.Configuration;

public class MediaStorageOptions
{
    public const string SectionName = "MediaStorage";

    public string ContainerName { get; set; } = "fitspire-media";
    public string? ConnectionString { get; set; }
    public string? ServiceUrl { get; set; }
    public int UploadSasMinutes { get; set; } = 15;
    public int ReadSasMinutes { get; set; } = 30;
    public int PendingUploadLifetimeMinutes { get; set; } = 60;
    public int CleanupIntervalMinutes { get; set; } = 15;
    public int ProcessingTimeoutMinutes { get; set; } = 120;
    public int UnattachedReadyLifetimeMinutes { get; set; } = 1_440;
    public int CleanupBatchSize { get; set; } = 50;
    public int MaxPendingUploadsPerUser { get; set; } = 20;
    public long ProfilePictureMaxBytes { get; set; } = 15 * 1024 * 1024;
    public long PostImageMaxBytes { get; set; } = 30 * 1024 * 1024;
    public long MaxDecodedPixels { get; set; } = 40_000_000;
    public int PostImageLimit { get; set; } = 10;

    public bool UsesConnectionString => !string.IsNullOrWhiteSpace(ConnectionString);
}
