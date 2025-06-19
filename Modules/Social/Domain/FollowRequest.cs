using backend.Modules.User.Domain;

namespace backend.Modules.Social.Domain;

public class FollowRequest
{
    public Guid Id { get; set; }

    public Guid RequesterId { get; set; }   // Who wants to follow
    public Guid AddresseeId { get; set; }   // Who is being followed

    public string Status { get; set; } = "pending"; // "pending", "accepted", "rejected"
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
    public DateTime? RespondedAt { get; set; }

    // Navigation
    public AppUser Requester { get; set; } = null!;
    public AppUser Addressee { get; set; } = null!;
}