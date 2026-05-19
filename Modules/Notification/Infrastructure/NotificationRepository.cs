using backend.Data;
using backend.Modules.Notification.Domain;
using Microsoft.EntityFrameworkCore;

namespace backend.Modules.Notification.Infrastructure;

public class NotificationRepository : INotificationRepository
{
    private readonly FitspireDbContext _context;

    public NotificationRepository(FitspireDbContext context)
    {
        _context = context;
    }

    public async Task<List<AppNotification>> GetForUserAsync(
        Guid userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        return await _context.Notifications
            .Where(notification => notification.UserId == userId)
            .OrderByDescending(notification => notification.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Notifications
            .CountAsync(notification => notification.UserId == userId && !notification.IsRead, cancellationToken);
    }

    public async Task<AppNotification?> GetByIdForUserAsync(
        Guid notificationId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Notifications
            .FirstOrDefaultAsync(
                notification => notification.Id == notificationId && notification.UserId == userId,
                cancellationToken);
    }

    public async Task AddAsync(AppNotification notification, CancellationToken cancellationToken = default)
    {
        await _context.Notifications.AddAsync(notification, cancellationToken);
    }

    public async Task<int> MarkAllReadAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Notifications
            .Where(notification => notification.UserId == userId && !notification.IsRead)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(notification => notification.IsRead, true)
                    .SetProperty(notification => notification.ReadAt, DateTime.UtcNow),
                cancellationToken);
    }
}
