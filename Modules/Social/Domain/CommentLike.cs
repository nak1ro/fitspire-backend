using backend.Modules.Shared.Domain;
using backend.Modules.User.Domain;

namespace backend.Modules.Social.Domain;

public class CommentLike : Entity<Guid>
{
    public Guid UserId { get; private set; }
    public Guid CommentId { get; private set; }

    public AppUser User { get; private set; } = null!;
    public Comment Comment { get; private set; } = null!;

    private CommentLike() { }

    public CommentLike(Guid userId, Guid commentId)
    {
        if (userId == Guid.Empty)
            throw new DomainException("User id is required.");
        if (commentId == Guid.Empty)
            throw new DomainException("Comment id is required.");

        Id = Guid.NewGuid();
        UserId = userId;
        CommentId = commentId;
        CreatedAt = DateTime.UtcNow;
    }
}
