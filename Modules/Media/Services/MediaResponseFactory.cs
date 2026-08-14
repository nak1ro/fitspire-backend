using backend.Modules.Media.Configuration;
using backend.Modules.Media.Contracts;
using backend.Modules.Media.Domain;
using Microsoft.Extensions.Options;

namespace backend.Modules.Media.Services;

public class MediaResponseFactory : IMediaResponseFactory
{
    private readonly IMediaObjectStorage _storage;
    private readonly MediaStorageOptions _options;

    public MediaResponseFactory(IMediaObjectStorage storage, IOptions<MediaStorageOptions> options)
    {
        _storage = storage;
        _options = options.Value;
    }

    public async Task<MediaResponse?> CreateAsync(MediaAsset? asset, CancellationToken cancellationToken)
    {
        if (asset is null || asset.IsModerationRemoved || asset.Status is not (MediaStatus.Ready or MediaStatus.Attached))
            return null;

        var response = await CreateManyAsync([asset], cancellationToken);
        return response.GetValueOrDefault(asset.Id);
    }

    public async Task<IReadOnlyDictionary<Guid, MediaResponse>> CreateManyAsync(
        IEnumerable<MediaAsset> assets,
        CancellationToken cancellationToken)
    {
        var mediaAssets = assets
            .Where(asset => !asset.IsModerationRemoved && asset.Status is (MediaStatus.Ready or MediaStatus.Attached))
            .DistinctBy(asset => asset.Id)
            .ToList();
        if (mediaAssets.Count == 0)
            return new Dictionary<Guid, MediaResponse>();

        var expiresAtUtc = DateTime.UtcNow.AddMinutes(_options.ReadSasMinutes);
        var responses = new Dictionary<Guid, MediaResponse>(mediaAssets.Count);
        foreach (var asset in mediaAssets)
            responses[asset.Id] = await CreateResponseAsync(asset, expiresAtUtc, cancellationToken);

        return responses;
    }

    private async Task<MediaResponse> CreateResponseAsync(
        MediaAsset asset,
        DateTime expiresAtUtc,
        CancellationToken cancellationToken)
    {
        var primary = await CreateVariantAsync(asset, MediaVariantKind.Primary, expiresAtUtc, cancellationToken);
        var thumbnail = await CreateVariantAsync(asset, MediaVariantKind.Thumbnail, expiresAtUtc, cancellationToken);
        return new MediaResponse(asset.Id, asset.Purpose, primary, thumbnail);
    }

    private async Task<MediaReadVariantResponse?> CreateVariantAsync(
        MediaAsset asset,
        MediaVariantKind kind,
        DateTime expiresAtUtc,
        CancellationToken cancellationToken)
    {
        var variant = asset.Variants.SingleOrDefault(candidate => candidate.Kind == kind);
        if (variant is null)
            return null;

        var sas = await _storage.CreateReadSasAsync(variant.BlobKey, expiresAtUtc, cancellationToken);
        return new MediaReadVariantResponse(sas.Url, sas.ExpiresAtUtc, variant.Width, variant.Height, variant.ContentType);
    }
}
