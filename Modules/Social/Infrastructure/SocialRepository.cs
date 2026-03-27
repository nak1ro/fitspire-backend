using backend.Data;
using backend.Modules.Social.Domain;
using backend.Modules.Social.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace backend.Modules.Social.Infrastructure;

public class SocialRepository : ISocialRepository
{
    private readonly FitspireDbContext _context;

    public SocialRepository(FitspireDbContext context)
    {
        _context = context;
    }

    // Posts
    public async Task<Post?> GetPostByIdAsync(Guid postId, CancellationToken cancellationToken = default)
    {
        return await _context.Posts
            .Include(p => p.User)
            .Include(p => p.Likes)
            .Include(p => p.Comments)
            .FirstOrDefaultAsync(p => p.Id == postId, cancellationToken);
    }

    public async Task<Post?> GetPostByReferenceAsync(PostType type, Guid referenceEntityId, CancellationToken cancellationToken = default)
    {
        return await _context.Posts
            .FirstOrDefaultAsync(
                p => p.Type == type && p.ReferenceEntityId == referenceEntityId,
                cancellationToken);
    }

    public async Task<List<Post>> GetUserFeedAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        // Get IDs of users this user follows
        var followedUserIds = await GetFollowedUserIdsAsync(userId, cancellationToken);
        
        // Include user's own posts in feed
        followedUserIds.Add(userId);

        return await _context.Posts
            .Include(p => p.User)
            .Include(p => p.Likes)
            .Include(p => p.Comments)
                .ThenInclude(c => c.User)
            .Where(p => followedUserIds.Contains(p.UserId))
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Post>> GetUserPostsAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        return await _context.Posts
            .Include(p => p.User)
            .Include(p => p.Likes)
            .Include(p => p.Comments)
                .ThenInclude(c => c.User)
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task AddPostAsync(Post post, CancellationToken cancellationToken = default)
    {
        await _context.Posts.AddAsync(post, cancellationToken);
    }

    public Task DeletePostAsync(Post post, CancellationToken cancellationToken = default)
    {
        _context.Posts.Remove(post);
        return Task.CompletedTask;
    }

    // Likes
    public async Task<Like?> GetLikeAsync(Guid userId, Guid targetId, CancellationToken cancellationToken = default)
    {
        return await _context.Likes
            .FirstOrDefaultAsync(
                l => l.UserId == userId && l.TargetId == targetId && l.TargetType == LikeTargetType.Post,
                cancellationToken);
    }

    public async Task AddLikeAsync(Like like, CancellationToken cancellationToken = default)
    {
        await _context.Likes.AddAsync(like, cancellationToken);
    }

    public Task RemoveLikeAsync(Like like, CancellationToken cancellationToken = default)
    {
        _context.Likes.Remove(like);
        return Task.CompletedTask;
    }

    // Comments
    public async Task AddCommentAsync(Comment comment, CancellationToken cancellationToken = default)
    {
        await _context.Comments.AddAsync(comment, cancellationToken);
    }

    public async Task<Comment?> GetCommentByIdAsync(Guid postId, Guid commentId, CancellationToken cancellationToken = default)
    {
        return await _context.Comments
            .Include(c => c.Post)
            .FirstOrDefaultAsync(c => c.PostId == postId && c.Id == commentId, cancellationToken);
    }

    public async Task<List<Comment>> GetPostCommentsAsync(Guid postId, CancellationToken cancellationToken = default)
    {
        return await _context.Comments
            .Include(c => c.User)
            .Where(c => c.PostId == postId)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public Task DeleteCommentAsync(Comment comment, CancellationToken cancellationToken = default)
    {
        _context.Comments.Remove(comment);
        return Task.CompletedTask;
    }

    // Follow
    public async Task<Follower?> GetFollowAsync(Guid followerId, Guid followedId, CancellationToken cancellationToken = default)
    {
        return await _context.Followers
            .FirstOrDefaultAsync(f => f.FollowerId == followerId && f.FollowedId == followedId, cancellationToken);
    }

    public async Task<List<Guid>> GetFollowedUserIdsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Followers
            .Where(f => f.FollowerId == userId)
            .Select(f => f.FollowedId)
            .ToListAsync(cancellationToken);
    }

    public async Task AddFollowerAsync(Follower follower, CancellationToken cancellationToken = default)
    {
        await _context.Followers.AddAsync(follower, cancellationToken);
    }

    public Task RemoveFollowerAsync(Follower follower, CancellationToken cancellationToken = default)
    {
        _context.Followers.Remove(follower);
        return Task.CompletedTask;
    }
}
