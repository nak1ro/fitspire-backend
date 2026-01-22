using backend.Modules.Shared.Domain;
using backend.Modules.Social.Domain.Enums;
using backend.Modules.User.Domain;

namespace backend.Modules.Social.Domain;

/// <summary>
/// Represents a like on a post or comment.
/// </summary>
public class Like : Entity<Guid>
{
    public Guid UserId { get; private set; }
    public Guid TargetId { get; private set; }
    public LikeTargetType TargetType { get; private set; }

    // Navigation
    public AppUser User { get; private set; } = null!;

    private Like() { }

    public static Like CreateForPost(Guid userId, Guid postId)
    {
        return new Like
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TargetId = postId,
            TargetType = LikeTargetType.Post,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static Like CreateForComment(Guid userId, Guid commentId)
    {
        return new Like
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TargetId = commentId,
            TargetType = LikeTargetType.Comment,
            CreatedAt = DateTime.UtcNow
        };
    }
}