using backend.Modules.User.Domain;

namespace backend.Modules.Challenge.Domain;

public class ChallengeInvitation
{
    public Guid Id { get; set; }
    public Guid ChallengeId { get; set; }
    public Guid InvitedUserId { get; set; }
    public Guid InvitedByUserId { get; set; }
    public string Status { get; set; } = "Pending";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? RespondedAt { get; set; }
    public UserChallenge Challenge { get; set; } = null!;
    public AppUser InvitedUser { get; set; } = null!;
}
