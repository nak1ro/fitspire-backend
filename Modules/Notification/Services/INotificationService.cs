using backend.Modules.Notification.Domain.Enums;
using backend.Modules.Notification.DTOs;

namespace backend.Modules.Notification.Services;

public interface INotificationService
{
    Task<IReadOnlyList<NotificationResponse>> GetNotificationsAsync(
        Guid userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<int> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken = default);
    Task MarkReadAsync(Guid userId, Guid notificationId, CancellationToken cancellationToken = default);
    Task<int> MarkAllReadAsync(Guid userId, CancellationToken cancellationToken = default);

    Task CreateAsync(
        Guid userId,
        NotificationType type,
        string message,
        Guid? actorUserId = null,
        Guid? referenceEntityId = null,
        string? referenceEntityType = null,
        CancellationToken cancellationToken = default);
}
