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
    public Guid? RootCommentId { get; private set; }
    public Guid? ReplyToCommentId { get; private set; }
    public DateTime? ModerationRemovedAtUtc { get; private set; }
    public bool IsModerationRemoved => ModerationRemovedAtUtc is not null;

    // Navigation
    public Post Post { get; private set; } = null!;
    public AppUser User { get; private set; } = null!;
    public Comment? RootComment { get; private set; }
    public Comment? ReplyToComment { get; private set; }
    public ICollection<Comment> Replies { get; private set; } = new List<Comment>();
    public ICollection<CommentLike> Likes { get; private set; } = new List<CommentLike>();

    private Comment() { }

    public Comment(Guid postId, Guid userId, string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new DomainException("Comment content is required.");

        Id = Guid.NewGuid();
        PostId = postId;
        UserId = userId;
        Content = content.Trim();
        CreatedAt = DateTime.UtcNow;
    }

    public static Comment CreateReply(Comment targetComment, Guid userId, string content)
    {
        var reply = new Comment(targetComment.PostId, userId, content)
        {
            RootCommentId = targetComment.RootCommentId ?? targetComment.Id,
            ReplyToCommentId = targetComment.Id
        };

        return reply;
    }

    public bool CanBeDeletedBy(Guid userId)
    {
        return UserId == userId || Post.UserId == userId;
    }

    public void UpdateContent(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new DomainException("Comment content is required.");

        Content = content.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    public void RemoveByModeration(DateTime utcNow)
    {
        if (utcNow.Kind != DateTimeKind.Utc)
            throw new DomainException("Moderation removal time must be in UTC.");
        if (ModerationRemovedAtUtc is not null)
            return;

        ModerationRemovedAtUtc = utcNow;
        UpdatedAt = utcNow;
    }

    public void RestoreByModeration(DateTime utcNow)
    {
        if (utcNow.Kind != DateTimeKind.Utc)
            throw new DomainException("Moderation restoration time must be in UTC.");
        if (ModerationRemovedAtUtc is null)
            return;

        ModerationRemovedAtUtc = null;
        UpdatedAt = utcNow;
    }
}
