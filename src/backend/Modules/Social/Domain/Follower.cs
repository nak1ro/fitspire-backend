using backend.Modules.User.Domain;

namespace backend.Modules.Social.Domain;

public class Follower
{
    public Guid Id { get; set; }

    public Guid FollowerId { get; set; }
    public AppUser FollowerUser { get; set; } = null!;

    public Guid FollowedId { get; set; }
    public AppUser FollowedUser { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}