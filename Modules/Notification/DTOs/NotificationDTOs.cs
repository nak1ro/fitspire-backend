namespace backend.Modules.Notification.DTOs;

public record NotificationResponse(
    Guid Id,
    string Type,
    string Message,
    Guid? ActorUserId,
    Guid? ReferenceEntityId,
    string? ReferenceEntityType,
    bool IsRead,
    DateTime? ReadAt,
    DateTime CreatedAt
);

public record UnreadNotificationCountResponse(int Count);

public record MarkAllNotificationsReadResponse(int Count);
