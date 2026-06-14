using backend.Modules.Shared.Domain;
using backend.Modules.User.Domain;

namespace backend.Modules.Social.Domain;

public class PostLike : Entity<Guid>
{
    public Guid UserId { get; private set; }
    public Guid PostId { get; private set; }

    public AppUser User { get; private set; } = null!;
    public Post Post { get; private set; } = null!;

    private PostLike() { }

    public PostLike(Guid userId, Guid postId)
    {
        if (userId == Guid.Empty)
            throw new DomainException("User id is required.");
        if (postId == Guid.Empty)
            throw new DomainException("Post id is required.");

        Id = Guid.NewGuid();
        UserId = userId;
        PostId = postId;
        CreatedAt = DateTime.UtcNow;
    }
}
