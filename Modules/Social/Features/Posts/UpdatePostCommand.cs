using backend.Data;
using backend.Modules.Media.Domain;
using backend.Modules.Shared.Domain;
using backend.Modules.Social.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace backend.Modules.Social.Features.Posts;

public record UpdatePostCommand(Guid UserId, Guid PostId, string? Content = null, IReadOnlyList<Guid>? MediaAssetIds = null) : IRequest;

public class UpdatePostHandler : IRequestHandler<UpdatePostCommand>
{
    private readonly FitspireDbContext _context;

    public UpdatePostHandler(FitspireDbContext context)
    {
        _context = context;
    }

    public async Task Handle(UpdatePostCommand request, CancellationToken cancellationToken)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        var post = await LoadOwnedPostAsync(request.PostId, request.UserId, cancellationToken);
        var existingMediaAssetIds = post.Media.Select(media => media.MediaAssetId).ToHashSet();
        var removedMediaIds = request.MediaAssetIds is null
            ? []
            : post.Media.Select(media => media.MediaAssetId).Except(request.MediaAssetIds).ToList();

        if (request.MediaAssetIds is not null)
            await ValidateNewMediaAsync(post, request.UserId, request.MediaAssetIds, cancellationToken);

        await StageMediaReorderAsync(post, request.MediaAssetIds, cancellationToken);
        post.UpdateTextPost(request.Content, request.MediaAssetIds);

        // PostMedia has a client-generated Guid key and new rows are only reachable here via
        // Post.ApplyMediaSet's Media.Add(...), a navigation fixup onto the already-loaded Media
        // collection (loaded above via Include). EF's default ValueGeneratedOnAdd heuristic then
        // marks a brand-new entry Modified instead of Added, producing an UPDATE for a row that
        // doesn't exist yet — the same class of bug fixed in MediaUploadService, MealWriteService,
        // and the gym workout mutation handlers.
        if (request.MediaAssetIds is not null)
            foreach (var media in post.Media.Where(media => !existingMediaAssetIds.Contains(media.MediaAssetId)))
                _context.Entry(media).State = EntityState.Added;

        await AttachNewMediaAsync(post, request.UserId, request.MediaAssetIds, cancellationToken);
        await RetireMediaAsync(removedMediaIds, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    private async Task StageMediaReorderAsync(
        Post post,
        IReadOnlyList<Guid>? requestedMediaIds,
        CancellationToken cancellationToken)
    {
        if (requestedMediaIds is null || !RequiresReorder(post, requestedMediaIds))
            return;

        foreach (var media in post.Media)
            media.MoveTo(MediaPolicies.MaximumPostImages + media.Order);

        await _context.SaveChangesAsync(cancellationToken);
    }

    private static bool RequiresReorder(Post post, IReadOnlyList<Guid> requestedMediaIds) =>
        post.Media.Any(media =>
        {
            var requestedOrder = requestedMediaIds
                .Select((mediaAssetId, index) => new { mediaAssetId, index })
                .FirstOrDefault(item => item.mediaAssetId == media.MediaAssetId)?.index ?? -1;
            return requestedOrder >= 0 && requestedOrder != media.Order;
        });

    private async Task<Post> LoadOwnedPostAsync(Guid postId, Guid userId, CancellationToken cancellationToken)
    {
        var post = await _context.Posts
            .Include(post => post.Media)
            .FirstOrDefaultAsync(post => post.Id == postId, cancellationToken)
            ?? throw new NotFoundException($"Post {postId} not found.");

        if (post.UserId != userId)
            throw new UnauthorizedAccessException("Post does not belong to the current user.");

        return post;
    }

    private async Task ValidateNewMediaAsync(
        Post post,
        Guid userId,
        IReadOnlyList<Guid> requestedMediaIds,
        CancellationToken cancellationToken)
    {
        var existingMediaIds = post.Media.Select(media => media.MediaAssetId).ToHashSet();
        var newMediaIds = requestedMediaIds.Where(id => !existingMediaIds.Contains(id)).ToList();
        if (newMediaIds.Count == 0)
            return;

        var assets = await _context.MediaAssets
            .Where(asset => asset.OwnerUserId == userId && newMediaIds.Contains(asset.Id))
            .ToListAsync(cancellationToken);

        if (assets.Count != newMediaIds.Count
            || assets.Any(asset => asset.Purpose != MediaPurpose.PostImage || asset.Status != MediaStatus.Ready))
            throw new DomainException("New post images must be owned, ready post-image uploads.");
    }

    private async Task AttachNewMediaAsync(
        Post post,
        Guid userId,
        IReadOnlyList<Guid>? requestedMediaIds,
        CancellationToken cancellationToken)
    {
        if (requestedMediaIds is null)
            return;

        var assets = await _context.MediaAssets
            .Where(asset => asset.OwnerUserId == userId && requestedMediaIds.Contains(asset.Id) && asset.Status == MediaStatus.Ready)
            .ToListAsync(cancellationToken);

        foreach (var asset in assets)
            asset.Attach(DateTime.UtcNow);
    }

    private async Task RetireMediaAsync(IReadOnlyCollection<Guid> mediaAssetIds, CancellationToken cancellationToken)
    {
        if (mediaAssetIds.Count == 0)
            return;

        var assets = await _context.MediaAssets.Where(asset => mediaAssetIds.Contains(asset.Id)).ToListAsync(cancellationToken);
        foreach (var asset in assets)
            asset.Retire(DateTime.UtcNow);
    }
}
