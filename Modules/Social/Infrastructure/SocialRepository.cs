using backend.Data;
using backend.Modules.Social.Domain;
using backend.Modules.Social.Domain.Enums;
using backend.Modules.User.Domain;
using Microsoft.EntityFrameworkCore;

namespace backend.Modules.Social.Infrastructure;

public class SocialRepository : ISocialRepository
{
    private readonly FitspireDbContext _context;

    public SocialRepository(FitspireDbContext context)
    {
        _context = context;
    }

    public async Task<bool> UserExistsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Users.AnyAsync(user => user.Id == userId, cancellationToken);
    }

    public async Task<string> GetUserDisplayNameAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users
            .Where(u => u.Id == userId)
            .Select(u => new { u.DisplayName, u.UserName })
            .FirstOrDefaultAsync(cancellationToken);

        return user?.DisplayName ?? user?.UserName ?? "Someone";
    }

    public Task<AppUser?> GetSocialUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return _context.Users.AsNoTracking().FirstOrDefaultAsync(user => user.Id == userId, cancellationToken);
    }

    public Task<List<AppUser>> SearchSocialUsersAsync(string query, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        return _context.Users.AsNoTracking()
            .Where(user => user.UserName!.Contains(query) || user.DisplayName.Contains(query))
            .OrderBy(user => user.UserName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public Task<int> GetFollowersCountAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return _context.Followers.CountAsync(follow => follow.FollowedId == userId, cancellationToken);
    }

    public Task<int> GetFollowingCountAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return _context.Followers.CountAsync(follow => follow.FollowerId == userId, cancellationToken);
    }

    public Task<bool> IsFollowingAsync(Guid followerId, Guid followedId, CancellationToken cancellationToken = default)
    {
        return _context.Followers.AnyAsync(
            follow => follow.FollowerId == followerId && follow.FollowedId == followedId,
            cancellationToken);
    }

    public Task<bool> HasPendingFollowRequestAsync(Guid requesterId, Guid addresseeId, CancellationToken cancellationToken = default)
    {
        return _context.FollowRequests.AnyAsync(
            request => request.RequesterId == requesterId &&
                       request.AddresseeId == addresseeId &&
                       request.Status == FollowRequestStatus.Pending,
            cancellationToken);
    }

    public async Task<bool> IsUserPrivateAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Users.Where(user => user.Id == userId)
            .Select(user => user.IsPrivate)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Task<FollowRequest?> GetFollowRequestAsync(Guid requestId, CancellationToken cancellationToken = default)
    {
        return _context.FollowRequests.FirstOrDefaultAsync(request => request.Id == requestId, cancellationToken);
    }

    public Task<FollowRequest?> GetPendingFollowRequestAsync(Guid requesterId, Guid addresseeId, CancellationToken cancellationToken = default)
    {
        return _context.FollowRequests.FirstOrDefaultAsync(
            request => request.RequesterId == requesterId && request.AddresseeId == addresseeId && request.Status == FollowRequestStatus.Pending,
            cancellationToken);
    }

    public Task AddFollowRequestAsync(FollowRequest request, CancellationToken cancellationToken = default)
    {
        return _context.FollowRequests.AddAsync(request, cancellationToken).AsTask();
    }

    public Task<List<FollowRequest>> GetIncomingFollowRequestsAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        return _context.FollowRequests.AsNoTracking()
            .Include(request => request.Requester)
            .Where(request => request.AddresseeId == userId && request.Status == FollowRequestStatus.Pending)
            .OrderByDescending(request => request.RequestedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public Task<List<FollowRequest>> GetOutgoingFollowRequestsAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        return _context.FollowRequests.AsNoTracking()
            .Include(request => request.Addressee)
            .Where(request => request.RequesterId == userId && request.Status == FollowRequestStatus.Pending)
            .OrderByDescending(request => request.RequestedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public Task<List<Follower>> GetFollowersAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        return _context.Followers.AsNoTracking()
            .Include(follow => follow.FollowerUser)
            .Where(follow => follow.FollowedId == userId)
            .OrderByDescending(follow => follow.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public Task<List<Follower>> GetFollowingAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        return _context.Followers.AsNoTracking()
            .Include(follow => follow.FollowedUser)
            .Where(follow => follow.FollowerId == userId)
            .OrderByDescending(follow => follow.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    // Posts
    public async Task<Post?> GetPostByIdAsync(Guid postId, CancellationToken cancellationToken = default)
    {
        return await _context.Posts
            .Include(p => p.User)
            .Include(p => p.Likes)
            .Include(p => p.Comments)
                .ThenInclude(comment => comment.User)
            .Include(p => p.Comments)
                .ThenInclude(comment => comment.Likes)
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
    public async Task<PostLike?> GetPostLikeAsync(Guid userId, Guid postId, CancellationToken cancellationToken = default)
    {
        return await _context.PostLikes
            .FirstOrDefaultAsync(
                l => l.UserId == userId && l.PostId == postId,
                cancellationToken);
    }

    public async Task AddPostLikeAsync(PostLike like, CancellationToken cancellationToken = default)
    {
        await _context.PostLikes.AddAsync(like, cancellationToken);
    }

    public Task RemovePostLikeAsync(PostLike like, CancellationToken cancellationToken = default)
    {
        _context.PostLikes.Remove(like);
        return Task.CompletedTask;
    }

    public Task<List<AppUser>> GetPostLikersAsync(Guid postId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        return _context.PostLikes.AsNoTracking()
            .Include(like => like.User)
            .Where(like => like.PostId == postId)
            .OrderByDescending(like => like.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(like => like.User)
            .ToListAsync(cancellationToken);
    }

    public Task<CommentLike?> GetCommentLikeAsync(Guid userId, Guid commentId, CancellationToken cancellationToken = default)
    {
        return _context.CommentLikes.FirstOrDefaultAsync(like => like.UserId == userId && like.CommentId == commentId, cancellationToken);
    }

    public Task AddCommentLikeAsync(CommentLike like, CancellationToken cancellationToken = default)
    {
        return _context.CommentLikes.AddAsync(like, cancellationToken).AsTask();
    }

    public Task RemoveCommentLikeAsync(CommentLike like, CancellationToken cancellationToken = default)
    {
        _context.CommentLikes.Remove(like);
        return Task.CompletedTask;
    }

    public Task<List<AppUser>> GetCommentLikersAsync(Guid commentId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        return _context.CommentLikes.AsNoTracking()
            .Include(like => like.User)
            .Where(like => like.CommentId == commentId)
            .OrderByDescending(like => like.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(like => like.User)
            .ToListAsync(cancellationToken);
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

    public Task<List<Comment>> GetTopLevelCommentsAsync(Guid postId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        return CommentDetailsQuery(postId)
            .Where(comment => comment.RootCommentId == null)
            .OrderByDescending(comment => comment.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public Task<List<Comment>> GetCommentRepliesAsync(Guid postId, Guid rootCommentId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        return CommentDetailsQuery(postId)
            .Where(comment => comment.RootCommentId == rootCommentId)
            .OrderBy(comment => comment.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<Dictionary<Guid, int>> GetReplyCountsAsync(
        Guid postId,
        IEnumerable<Guid> rootCommentIds,
        CancellationToken cancellationToken = default)
    {
        var ids = rootCommentIds.Distinct().ToList();
        return await _context.Comments.AsNoTracking()
            .Where(comment => comment.PostId == postId
                              && comment.RootCommentId.HasValue
                              && ids.Contains(comment.RootCommentId.Value))
            .GroupBy(comment => comment.RootCommentId!.Value)
            .Select(group => new { RootId = group.Key, Count = group.Count() })
            .ToDictionaryAsync(item => item.RootId, item => item.Count, cancellationToken);
    }

    private IQueryable<Comment> CommentDetailsQuery(Guid postId)
    {
        return _context.Comments.AsNoTracking()
            .Include(comment => comment.User)
            .Include(comment => comment.Likes)
            .Where(comment => comment.PostId == postId);
    }

    public Task<List<Post>> GetDiscoverFeedAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        return _context.Posts
            .Include(post => post.User)
            .Include(post => post.Likes)
            .Include(post => post.Comments)
                .ThenInclude(comment => comment.User)
            .Where(post => !post.User.IsPrivate)
            .OrderByDescending(post => post.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
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
