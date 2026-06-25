using backend.Modules.User.Domain;

namespace backend.Modules.Challenge.Domain;

public class ChallengeResult
{
    public Guid Id { get; set; }
    public Guid ChallengeId { get; set; }
    public Guid ParticipantId { get; set; }
    public Guid UserId { get; set; }
    public double Score { get; set; }
    public int Rank { get; set; }
    public bool IsFinisher { get; set; }
    public bool IsWinner { get; set; }
    public DateTime FinalizedAt { get; set; } = DateTime.UtcNow;
    public UserChallenge Challenge { get; set; } = null!;
    public AppUser User { get; set; } = null!;
}
