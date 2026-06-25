using backend.Modules.User.Domain;
using backend.Modules.Challenge.Domain.Constants;
using backend.Modules.Shared.Domain;

namespace backend.Modules.Challenge.Domain;


public class ChallengeParticipant
{
    public Guid Id { get; private set; }

    public Guid ChallengeId { get; private set; }
    public Guid UserId { get; private set; }

    public float Score { get; private set; }
    public DateTime JoinedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? LeftAt { get; private set; }
    public string Status { get; private set; } = ChallengeParticipantStatuses.Active;

    // Navigation
    public AppUser User { get; set; } = null!;
    public UserChallenge UserChallenge { get; set; } = null!;

    private ChallengeParticipant() { }

    public static ChallengeParticipant Create(Guid challengeId, Guid userId, DateTime joinedAt) => new()
    {
        Id = Guid.NewGuid(), ChallengeId = challengeId, UserId = userId, JoinedAt = joinedAt, Status = ChallengeParticipantStatuses.Active
    };

    public void Reactivate(DateTime joinedAt)
    {
        Status = ChallengeParticipantStatuses.Active;
        JoinedAt = joinedAt;
        LeftAt = null;
        Score = 0;
    }

    public void Leave(DateTime nowUtc)
    {
        if (Status != ChallengeParticipantStatuses.Active) throw new DomainException("Only active participants can leave.");
        Status = ChallengeParticipantStatuses.Left;
        LeftAt = nowUtc;
    }

    public void Remove(DateTime nowUtc)
    {
        if (Status != ChallengeParticipantStatuses.Active) throw new DomainException("Only active participants can be removed.");
        Status = ChallengeParticipantStatuses.Removed;
        LeftAt = nowUtc;
    }

    public void SetScore(double score)
    {
        Score = (float)Math.Max(0, score);
    }
}
