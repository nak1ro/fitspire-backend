namespace backend.Modules.Media.Contracts;

public interface IMediaUploadService
{
    Task<MediaUploadSessionResponse> InitiateAsync(InitiateMediaUploadRequest request, Guid userId, CancellationToken cancellationToken);
    Task<MediaAssetStatusResponse> CompleteAsync(Guid mediaAssetId, Guid userId, CancellationToken cancellationToken);
    Task<MediaAssetStatusResponse> GetStatusAsync(Guid mediaAssetId, Guid userId, CancellationToken cancellationToken);
    Task AbortAsync(Guid mediaAssetId, Guid userId, CancellationToken cancellationToken);
}
