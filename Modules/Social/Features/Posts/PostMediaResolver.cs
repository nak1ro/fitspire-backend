using backend.Data;
using backend.Modules.Media.Domain;
using backend.Modules.Shared.Domain;
using Microsoft.EntityFrameworkCore;

namespace backend.Modules.Social.Features.Posts;

internal static class PostMediaResolver
{
    public static async Task<List<MediaAsset>> LoadReadyPostMediaAsync(
        FitspireDbContext context,
        Guid userId,
        IReadOnlyList<Guid> mediaAssetIds,
        CancellationToken cancellationToken)
    {
        if (mediaAssetIds.Count == 0)
            return [];

        var assets = await context.MediaAssets
            .Where(asset => asset.OwnerUserId == userId && mediaAssetIds.Contains(asset.Id))
            .ToListAsync(cancellationToken);

        if (assets.Count != mediaAssetIds.Count)
            throw new NotFoundException("One or more media uploads were not found.");

        if (assets.Any(asset => asset.Purpose != MediaPurpose.PostImage || asset.Status != MediaStatus.Ready))
            throw new DomainException("Post images must be owned, ready post-image uploads.");

        return assets;
    }
}
