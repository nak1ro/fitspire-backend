using backend.Data;
using backend.Modules.Media.Domain;
using backend.Modules.Shared.Domain;
using backend.Modules.Social.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace backend.Modules.Social.Features.Posts;

public record CreatePostCommand(Guid UserId, string? Content, IReadOnlyList<Guid>? MediaAssetIds = null) : IRequest<Guid>;

public class CreatePostHandler : IRequestHandler<CreatePostCommand, Guid>
{
    private readonly FitspireDbContext _context;

    public CreatePostHandler(FitspireDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreatePostCommand request, CancellationToken cancellationToken)
    {
        var mediaAssetIds = request.MediaAssetIds ?? [];
        await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);

        var assets = await LoadReadyPostMediaAsync(request.UserId, mediaAssetIds, cancellationToken);
        var post = Post.CreateTextPost(request.UserId, request.Content, mediaAssetIds);
        foreach (var asset in assets)
            asset.Attach(DateTime.UtcNow);

        _context.Posts.Add(post);
        await _context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return post.Id;
    }

    private async Task<List<MediaAsset>> LoadReadyPostMediaAsync(
        Guid userId,
        IReadOnlyList<Guid> mediaAssetIds,
        CancellationToken cancellationToken)
    {
        if (mediaAssetIds.Count == 0)
            return [];

        var assets = await _context.MediaAssets
            .Where(asset => asset.OwnerUserId == userId && mediaAssetIds.Contains(asset.Id))
            .ToListAsync(cancellationToken);

        if (assets.Count != mediaAssetIds.Count)
            throw new NotFoundException("One or more media uploads were not found.");

        if (assets.Any(asset => asset.Purpose != MediaPurpose.PostImage || asset.Status != MediaStatus.Ready))
            throw new DomainException("Post images must be owned, ready post-image uploads.");

        return assets;
    }
}
