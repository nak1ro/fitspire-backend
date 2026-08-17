using backend.Data;
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

        var assets = await PostMediaResolver.LoadReadyPostMediaAsync(_context, request.UserId, mediaAssetIds, cancellationToken);
        var post = Post.CreateTextPost(request.UserId, request.Content, mediaAssetIds);
        foreach (var asset in assets)
            asset.Attach(DateTime.UtcNow);

        _context.Posts.Add(post);
        await _context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return post.Id;
    }
}
