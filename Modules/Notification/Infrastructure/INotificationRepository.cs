using backend.Modules.Notification.Domain;

namespace backend.Modules.Notification.Infrastructure;

public interface INotificationRepository
{
    Task<List<AppNotification>> GetForUserAsync(
        Guid userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<int> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<AppNotification?> GetByIdForUserAsync(Guid notificationId, Guid userId, CancellationToken cancellationToken = default);
    Task AddAsync(AppNotification notification, CancellationToken cancellationToken = default);
    Task<int> MarkAllReadAsync(Guid userId, CancellationToken cancellationToken = default);
}
