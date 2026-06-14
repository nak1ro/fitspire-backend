using backend.Modules.User.Domain;
using backend.Modules.Shared.Domain;
using backend.Modules.Social.Domain.Enums;

namespace backend.Modules.Social.Domain;

public class FollowRequest : Entity<Guid>
{
    public Guid RequesterId { get; private set; }
    public Guid AddresseeId { get; private set; }
    public FollowRequestStatus Status { get; private set; } = FollowRequestStatus.Pending;
    public DateTime RequestedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? RespondedAt { get; private set; }

    // Navigation
    public AppUser Requester { get; private set; } = null!;
    public AppUser Addressee { get; private set; } = null!;

    private FollowRequest() { }

    public FollowRequest(Guid requesterId, Guid addresseeId)
    {
        if (requesterId == Guid.Empty || addresseeId == Guid.Empty)
            throw new DomainException("Follow request users are required.");
        if (requesterId == addresseeId)
            throw new DomainException("Users cannot follow themselves.");

        Id = Guid.NewGuid();
        RequesterId = requesterId;
        AddresseeId = addresseeId;
        RequestedAt = DateTime.UtcNow;
        CreatedAt = RequestedAt;
    }

    public void Accept() => TransitionTo(FollowRequestStatus.Accepted);
    public void Reject() => TransitionTo(FollowRequestStatus.Rejected);
    public void Cancel() => TransitionTo(FollowRequestStatus.Cancelled);

    private void TransitionTo(FollowRequestStatus targetStatus)
    {
        if (Status != FollowRequestStatus.Pending)
            throw new DomainException("Only pending follow requests can be changed.");

        Status = targetStatus;
        RespondedAt = DateTime.UtcNow;
        UpdatedAt = RespondedAt;
    }
}
