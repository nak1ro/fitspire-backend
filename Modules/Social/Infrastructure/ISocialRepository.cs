using backend.Modules.Social.Domain;
using backend.Modules.Social.Domain.Enums;

namespace backend.Modules.Social.Infrastructure;

public interface ISocialRepository
{
    // Posts
    Task<Post?> GetPostByIdAsync(Guid postId, CancellationToken cancellationToken = default);
    Task<Post?> GetPostByReferenceAsync(PostType type, Guid referenceEntityId, CancellationToken cancellationToken = default);
    Task<List<Post>> GetUserFeedAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<List<Post>> GetUserPostsAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task AddPostAsync(Post post, CancellationToken cancellationToken = default);
    
    // Likes
    Task<Like?> GetLikeAsync(Guid userId, Guid targetId, CancellationToken cancellationToken = default);
    Task AddLikeAsync(Like like, CancellationToken cancellationToken = default);
    Task RemoveLikeAsync(Like like, CancellationToken cancellationToken = default);
    
    // Comments
    Task AddCommentAsync(Comment comment, CancellationToken cancellationToken = default);
    Task<List<Comment>> GetPostCommentsAsync(Guid postId, CancellationToken cancellationToken = default);
    
    // Follow
    Task<Follower?> GetFollowAsync(Guid followerId, Guid followedId, CancellationToken cancellationToken = default);
    Task<List<Guid>> GetFollowedUserIdsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task AddFollowerAsync(Follower follower, CancellationToken cancellationToken = default);
    Task RemoveFollowerAsync(Follower follower, CancellationToken cancellationToken = default);
}
