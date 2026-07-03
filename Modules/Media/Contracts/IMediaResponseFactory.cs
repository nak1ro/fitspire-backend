using backend.Modules.Media.Domain;

namespace backend.Modules.Media.Contracts;

public interface IMediaResponseFactory
{
    Task<MediaResponse?> CreateAsync(MediaAsset? asset, CancellationToken cancellationToken);
    Task<IReadOnlyDictionary<Guid, MediaResponse>> CreateManyAsync(
        IEnumerable<MediaAsset> assets,
        CancellationToken cancellationToken);
}
