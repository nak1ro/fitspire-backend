namespace backend.Modules.Media.Contracts;

public interface IMediaObjectStorage
{
    Task<MediaUploadSasResult> CreateUploadSasAsync(
        string blobKey,
        string contentType,
        DateTime expiresAtUtc,
        CancellationToken cancellationToken);

    Task<MediaReadSasResult> CreateReadSasAsync(
        string blobKey,
        DateTime expiresAtUtc,
        CancellationToken cancellationToken);

    Task<MediaStoredObjectInfo?> GetObjectInfoAsync(string blobKey, CancellationToken cancellationToken);
    Task<Stream> OpenReadAsync(string blobKey, CancellationToken cancellationToken);
    Task UploadAsync(string blobKey, Stream content, string contentType, CancellationToken cancellationToken);
    Task DeleteIfExistsAsync(string blobKey, CancellationToken cancellationToken);
}

public record MediaUploadSasResult(string Url, DateTime ExpiresAtUtc);
public record MediaReadSasResult(string Url, DateTime ExpiresAtUtc);
public record MediaStoredObjectInfo(long SizeBytes, string ETag, string ContentType);
