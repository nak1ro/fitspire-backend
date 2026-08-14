using backend.Modules.Media.Configuration;
using backend.Modules.Media.Contracts;
using backend.Modules.Media.Domain;
using Microsoft.Extensions.Options;

namespace backend.Modules.Moderation.Services;

public interface IModerationMediaPreviewService
{
    Task<MediaResponse?> CreateAsync(MediaAsset? asset, CancellationToken cancellationToken = default);
}

public sealed class ModerationMediaPreviewService : IModerationMediaPreviewService
{
    private readonly IMediaObjectStorage _storage;
    private readonly MediaStorageOptions _options;

    public ModerationMediaPreviewService(IMediaObjectStorage storage, IOptions<MediaStorageOptions> options)
    {
        _storage = storage;
        _options = options.Value;
    }

    public async Task<MediaResponse?> CreateAsync(MediaAsset? asset, CancellationToken cancellationToken = default)
    {
        if (asset is null || asset.Status is not (MediaStatus.Ready or MediaStatus.Attached))
            return null;

        var expiresAtUtc = DateTime.UtcNow.AddMinutes(_options.ReadSasMinutes);
        var primary = await CreateVariantAsync(asset, MediaVariantKind.Primary, expiresAtUtc, cancellationToken);
        var thumbnail = await CreateVariantAsync(asset, MediaVariantKind.Thumbnail, expiresAtUtc, cancellationToken);
        return new MediaResponse(asset.Id, asset.Purpose, primary, thumbnail);
    }

    private async Task<MediaReadVariantResponse?> CreateVariantAsync(MediaAsset asset, MediaVariantKind kind,
        DateTime expiresAtUtc, CancellationToken cancellationToken)
    {
        var variant = asset.Variants.SingleOrDefault(candidate => candidate.Kind == kind);
        if (variant is null)
            return null;

        var sas = await _storage.CreateReadSasAsync(variant.BlobKey, expiresAtUtc, cancellationToken);
        return new MediaReadVariantResponse(sas.Url, sas.ExpiresAtUtc, variant.Width, variant.Height, variant.ContentType);
    }
}
