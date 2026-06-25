using backend.Modules.User.Domain;
using backend.Modules.Challenge.Domain.Constants;
using backend.Modules.Shared.Domain;

namespace backend.Modules.Challenge.Domain;

public class ChallengeInvitation
{
    public Guid Id { get; private set; }
    public Guid ChallengeId { get; private set; }
    public Guid InvitedUserId { get; private set; }
    public Guid InvitedByUserId { get; private set; }
    public string Status { get; private set; } = ChallengeInvitationStatuses.Pending;
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? RespondedAt { get; private set; }
    public UserChallenge Challenge { get; set; } = null!;
    public AppUser InvitedUser { get; set; } = null!;

    private ChallengeInvitation() { }

    public static ChallengeInvitation Create(Guid challengeId, Guid invitedUserId, Guid invitedByUserId, DateTime nowUtc) => new()
    {
        Id = Guid.NewGuid(), ChallengeId = challengeId, InvitedUserId = invitedUserId, InvitedByUserId = invitedByUserId, CreatedAt = nowUtc
    };

    public void Accept(DateTime nowUtc)
    {
        EnsurePending();
        Status = ChallengeInvitationStatuses.Accepted;
        RespondedAt = nowUtc;
    }

    public void Reject(DateTime nowUtc)
    {
        EnsurePending();
        Status = ChallengeInvitationStatuses.Rejected;
        RespondedAt = nowUtc;
    }

    public void Cancel(DateTime nowUtc)
    {
        EnsurePending();
        Status = ChallengeInvitationStatuses.Cancelled;
        RespondedAt = nowUtc;
    }

    public void Expire(DateTime nowUtc)
    {
        if (Status != ChallengeInvitationStatuses.Pending) return;
        Status = ChallengeInvitationStatuses.Expired;
        RespondedAt = nowUtc;
    }

    public void Reopen(DateTime nowUtc)
    {
        Status = ChallengeInvitationStatuses.Pending;
        RespondedAt = null;
        CreatedAt = nowUtc;
    }

    private void EnsurePending()
    {
        if (Status != ChallengeInvitationStatuses.Pending)
            throw new DomainException("Only pending invitations can be changed.");
    }
}
