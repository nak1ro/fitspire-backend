using backend.Modules.User.Domain;

namespace backend.Modules.Friendship.Domain;

public class FriendshipRequest
{
    public Guid Id { get; set; }

    public Guid RequesterId { get; set; }
    public AppUser Requester { get; set; } = null!;

    public Guid AddresseeId { get; set; }
    public AppUser Addressee { get; set; } = null!;

    public string Status { get; set; } = "pending"; // "pending", "accepted", "rejected"
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
    public DateTime? RespondedAt { get; set; }
}