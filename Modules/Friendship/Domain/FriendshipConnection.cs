using backend.Modules.User.Domain;

namespace backend.Modules.Friendship.Domain;

public class FriendshipConnection
{
    public Guid Id { get; set; }

    public Guid User1Id { get; set; }
    public AppUser User1 { get; set; } = null!;

    public Guid User2Id { get; set; }
    public AppUser User2 { get; set; } = null!;

    public DateTime BecameFriendsAt { get; set; } = DateTime.UtcNow;
}
