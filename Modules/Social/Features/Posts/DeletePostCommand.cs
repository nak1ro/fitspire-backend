using backend.Data;
using backend.Modules.Shared.Domain;
using backend.Modules.Social.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace backend.Modules.Social.Features.Posts;

public record DeletePostCommand(Guid UserId, Guid PostId) : IRequest;

public class DeletePostHandler : IRequestHandler<DeletePostCommand>
{
    private readonly FitspireDbContext _context;

    public DeletePostHandler(FitspireDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeletePostCommand request, CancellationToken cancellationToken)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        var post = await LoadOwnedPostAsync(request.PostId, request.UserId, cancellationToken);
        var mediaIds = post.Media.Select(media => media.MediaAssetId).ToList();

        var assets = await _context.MediaAssets.Where(asset => mediaIds.Contains(asset.Id)).ToListAsync(cancellationToken);
        foreach (var asset in assets)
            asset.Retire(DateTime.UtcNow);

        _context.Posts.Remove(post);
        await _context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    private async Task<Post> LoadOwnedPostAsync(Guid postId, Guid userId, CancellationToken cancellationToken)
    {
        var post = await _context.Posts
            .Include(post => post.Media)
            .FirstOrDefaultAsync(post => post.Id == postId && post.ModerationRemovedAtUtc == null, cancellationToken)
            ?? throw new NotFoundException($"Post {postId} not found.");

        if (post.UserId != userId)
            throw new UnauthorizedAccessException("Post does not belong to the current user.");

        return post;
    }
}
