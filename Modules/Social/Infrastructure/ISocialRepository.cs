using backend.Modules.Social.Domain;
using backend.Modules.Social.Domain.Enums;
using backend.Modules.User.Domain;

namespace backend.Modules.Social.Infrastructure;

public interface ISocialRepository
{
    Task<bool> UserExistsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<string> GetUserDisplayNameAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<AppUser?> GetSocialUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<List<AppUser>> SearchSocialUsersAsync(string query, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<List<AppUser>> SearchDiscoverableUsersAsync(Guid viewerUserId, string query, int limit, CancellationToken cancellationToken = default);
    Task<List<AppUser>> GetRecommendedUsersAsync(Guid viewerUserId, int limit, CancellationToken cancellationToken = default);
    Task<int> GetFollowersCountAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<int> GetFollowingCountAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> IsFollowingAsync(Guid followerId, Guid followedId, CancellationToken cancellationToken = default);
    Task<bool> HasPendingFollowRequestAsync(Guid requesterId, Guid addresseeId, CancellationToken cancellationToken = default);
    Task<bool> IsUserPrivateAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<FollowRequest?> GetFollowRequestAsync(Guid requestId, CancellationToken cancellationToken = default);
    Task<FollowRequest?> GetPendingFollowRequestAsync(Guid requesterId, Guid addresseeId, CancellationToken cancellationToken = default);
    Task AddFollowRequestAsync(FollowRequest request, CancellationToken cancellationToken = default);
    Task<List<FollowRequest>> GetIncomingFollowRequestsAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<List<FollowRequest>> GetOutgoingFollowRequestsAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<List<Follower>> GetFollowersAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<List<Follower>> GetFollowingAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken = default);

    // Posts
    Task<Post?> GetPostByIdAsync(Guid postId, CancellationToken cancellationToken = default);
    Task<Post?> GetPostByReferenceAsync(PostType type, Guid referenceEntityId, CancellationToken cancellationToken = default);
    Task<List<Guid>> GetSharedWorkoutIdsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<List<Guid>> GetSharedGoalIdsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Post?> GetPersonalRecordSharePostAsync(Guid personalRecordId, DateTime achievedAt, CancellationToken cancellationToken = default);
    Task<List<(Guid PersonalRecordId, DateTime AchievedAt)>> GetSharedPersonalRecordPairsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<List<Post>> GetUserFeedAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<List<Post>> GetDiscoverFeedAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<List<Post>> GetUserPostsAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task AddPostAsync(Post post, CancellationToken cancellationToken = default);
    Task DeletePostAsync(Post post, CancellationToken cancellationToken = default);
    
    // Likes
    Task<PostLike?> GetPostLikeAsync(Guid userId, Guid postId, CancellationToken cancellationToken = default);
    Task AddPostLikeAsync(PostLike like, CancellationToken cancellationToken = default);
    Task RemovePostLikeAsync(PostLike like, CancellationToken cancellationToken = default);
    Task<List<AppUser>> GetPostLikersAsync(Guid postId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<CommentLike?> GetCommentLikeAsync(Guid userId, Guid commentId, CancellationToken cancellationToken = default);
    Task AddCommentLikeAsync(CommentLike like, CancellationToken cancellationToken = default);
    Task RemoveCommentLikeAsync(CommentLike like, CancellationToken cancellationToken = default);
    Task<List<AppUser>> GetCommentLikersAsync(Guid commentId, int page, int pageSize, CancellationToken cancellationToken = default);
    
    // Comments
    Task AddCommentAsync(Comment comment, CancellationToken cancellationToken = default);
    Task<Comment?> GetCommentByIdAsync(Guid postId, Guid commentId, CancellationToken cancellationToken = default);
    Task<List<Comment>> GetTopLevelCommentsAsync(Guid postId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<List<Comment>> GetCommentRepliesAsync(Guid postId, Guid rootCommentId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<Dictionary<Guid, int>> GetReplyCountsAsync(Guid postId, IEnumerable<Guid> rootCommentIds, CancellationToken cancellationToken = default);
    Task DeleteCommentAsync(Comment comment, CancellationToken cancellationToken = default);
    
    // Follow
    Task<Follower?> GetFollowAsync(Guid followerId, Guid followedId, CancellationToken cancellationToken = default);
    Task<List<Guid>> GetFollowedUserIdsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task AddFollowerAsync(Follower follower, CancellationToken cancellationToken = default);
    Task RemoveFollowerAsync(Follower follower, CancellationToken cancellationToken = default);
}
