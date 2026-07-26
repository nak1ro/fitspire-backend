using backend.Data;
using backend.Modules.Media.Contracts;
using backend.Modules.Shared.Domain;
using backend.Modules.Social.Contracts.Posts;
using backend.Modules.Social.Domain;
using backend.Modules.Social.Features.Common;
using backend.Modules.Social.Services;
using backend.Modules.Workout.Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace backend.Modules.Social.Features.Posts;

public record SavePostCommand(Guid UserId, Guid PostId, bool ShouldSave) : IRequest<SavePostResponse>;
public record GetSavedPostsQuery(Guid UserId, int Page, int PageSize) : IRequest<List<FeedItemResponse>>;

public class SavePostHandler : IRequestHandler<SavePostCommand, SavePostResponse>
{
    private readonly FitspireDbContext _context;
    private readonly ISocialAccessService _access;

    public SavePostHandler(FitspireDbContext context, ISocialAccessService access)
    {
        _context = context;
        _access = access;
    }

    public async Task<SavePostResponse> Handle(SavePostCommand request, CancellationToken cancellationToken)
    {
        var post = await _context.Posts.Include(item => item.User)
            .FirstOrDefaultAsync(item => item.Id == request.PostId, cancellationToken)
            ?? throw new NotFoundException("Post not found.");
        if (!await _access.CanViewProtectedContentAsync(request.UserId, post.UserId, cancellationToken))
            throw new NotFoundException("Post not found.");

        var savedPost = await _context.SavedPosts.FirstOrDefaultAsync(item => item.UserId == request.UserId && item.PostId == request.PostId, cancellationToken);
        if (request.ShouldSave && savedPost is null)
            await _context.SavedPosts.AddAsync(new SavedPost { Id = Guid.NewGuid(), UserId = request.UserId, PostId = request.PostId }, cancellationToken);
        if (!request.ShouldSave && savedPost is not null)
            _context.SavedPosts.Remove(savedPost);

        await _context.SaveChangesAsync(cancellationToken);
        return new SavePostResponse(request.ShouldSave);
    }
}

public class GetSavedPostsHandler : IRequestHandler<GetSavedPostsQuery, List<FeedItemResponse>>
{
    private readonly FitspireDbContext _context;
    private readonly IWorkoutRepository _workouts;
    private readonly IMediaResponseFactory _media;

    public GetSavedPostsHandler(FitspireDbContext context, IWorkoutRepository workouts, IMediaResponseFactory media)
    {
        _context = context;
        _workouts = workouts;
        _media = media;
    }

    public async Task<List<FeedItemResponse>> Handle(GetSavedPostsQuery request, CancellationToken cancellationToken)
    {
        var posts = await _context.SavedPosts.Where(item => item.UserId == request.UserId)
            .OrderByDescending(item => item.SavedAt).Select(item => item.Post)
            .Include(post => post.User).ThenInclude(user => user.ProfilePictureMedia).ThenInclude(media => media!.Variants)
            .Include(post => post.Media).ThenInclude(media => media.MediaAsset).ThenInclude(asset => asset.Variants)
            .Include(post => post.Likes).Include(post => post.Comments).ThenInclude(comment => comment.User)
            .Skip((request.Page - 1) * request.PageSize).Take(request.PageSize).ToListAsync(cancellationToken);
        return await PostResponseMapper.MapAsync(posts, request.UserId, _workouts, _media, cancellationToken);
    }
}
