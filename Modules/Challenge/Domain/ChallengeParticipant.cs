using backend.Modules.User.Domain;

namespace backend.Modules.Challenge.Domain;


public class ChallengeParticipant
{
    public Guid Id { get; set; }

    public Guid ChallengeId { get; set; }
    public Guid UserId { get; set; }

    public float Score { get; set; }

    // Navigation
    public AppUser User { get; set; } = null!;
    public UserChallenge UserChallenge { get; set; } = null!;
}