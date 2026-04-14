using backend.Modules.Shared.Domain;
using backend.Modules.User.Domain;

namespace backend.Modules.Social.Domain;

/// <summary>
/// Represents a follow relationship between two users.
/// </summary>
public class Follower : Entity<Guid>
{
    public Guid FollowerId { get; private set; }
    public AppUser FollowerUser { get; private set; } = null!;

    public Guid FollowedId { get; private set; }
    public AppUser FollowedUser { get; private set; } = null!;

    private Follower() { }

    public Follower(Guid followerId, Guid followedId)
    {
        if (followerId == followedId)
            throw new DomainException("Users cannot follow themselves.");

        Id = Guid.NewGuid();
        FollowerId = followerId;
        FollowedId = followedId;
        CreatedAt = DateTime.UtcNow;
    }
}
