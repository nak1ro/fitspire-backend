using backend.Modules.Shared.Domain;
using backend.Modules.User.Domain;

namespace backend.Modules.Social.Domain;

/// <summary>
/// Represents a comment on a post.
/// </summary>
public class Comment : Entity<Guid>
{
    public Guid PostId { get; private set; }
    public Guid UserId { get; private set; }
    public string Content { get; private set; } = null!;

    // Navigation
    public Post Post { get; private set; } = null!;
    public AppUser User { get; private set; } = null!;

    private Comment() { }

    public Comment(Guid postId, Guid userId, string content)
    {
        Id = Guid.NewGuid();
        PostId = postId;
        UserId = userId;
        Content = content;
        CreatedAt = DateTime.UtcNow;
    }

    public bool CanBeDeletedBy(Guid userId)
    {
        return UserId == userId || Post.UserId == userId;
    }
}
