using backend.Modules.User.Domain;
using backend.Modules.Notification.Domain.Enums;
using backend.Modules.Shared.Domain;

namespace backend.Modules.Notification.Domain;

public class AppNotification
{
    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }
    public NotificationType Type { get; private set; }
    public string Message { get; private set; } = null!;
    public Guid? ActorUserId { get; private set; }
    public Guid? ReferenceEntityId { get; private set; }
    public string? ReferenceEntityType { get; private set; }
    public bool IsRead { get; private set; }
    public DateTime? ReadAt { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    // Navigation
    public AppUser User { get; private set; } = null!;

    private AppNotification() { }

    public AppNotification(
        Guid id,
        Guid userId,
        NotificationType type,
        string message,
        Guid? actorUserId = null,
        Guid? referenceEntityId = null,
        string? referenceEntityType = null)
    {
        if (id == Guid.Empty)
            throw new DomainException("Notification id is required.");

        if (userId == Guid.Empty)
            throw new DomainException("Notification recipient is required.");

        if (string.IsNullOrWhiteSpace(message))
            throw new DomainException("Notification message is required.");

        Id = id;
        UserId = userId;
        Type = type;
        Message = message.Trim();
        ActorUserId = actorUserId;
        ReferenceEntityId = referenceEntityId;
        ReferenceEntityType = string.IsNullOrWhiteSpace(referenceEntityType)
            ? null
            : referenceEntityType.Trim();
        CreatedAt = DateTime.UtcNow;
    }

    public void MarkRead()
    {
        if (IsRead)
            return;

        IsRead = true;
        ReadAt = DateTime.UtcNow;
    }
}
