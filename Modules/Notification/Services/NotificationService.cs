using AutoMapper;
using backend.Modules.Notification.Domain;
using backend.Modules.Notification.Domain.Enums;
using backend.Modules.Notification.DTOs;
using backend.Modules.Notification.Infrastructure;
using backend.Modules.Shared;
using backend.Modules.Shared.Domain;

namespace backend.Modules.Notification.Services;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _notificationRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public NotificationService(
        INotificationRepository notificationRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _notificationRepository = notificationRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<NotificationResponse>> GetNotificationsAsync(
        Guid userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var notifications = await _notificationRepository.GetForUserAsync(userId, page, pageSize, cancellationToken);
        return _mapper.Map<IReadOnlyList<NotificationResponse>>(notifications);
    }

    public Task<int> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return _notificationRepository.GetUnreadCountAsync(userId, cancellationToken);
    }

    public async Task MarkReadAsync(Guid userId, Guid notificationId, CancellationToken cancellationToken = default)
    {
        var notification = await _notificationRepository.GetByIdForUserAsync(notificationId, userId, cancellationToken);
        if (notification is null)
            throw new NotFoundException($"Notification {notificationId} not found.");

        notification.MarkRead();
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> MarkAllReadAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _notificationRepository.MarkAllReadAsync(userId, cancellationToken);
    }

    public async Task CreateAsync(
        Guid userId,
        NotificationType type,
        string message,
        Guid? actorUserId = null,
        Guid? referenceEntityId = null,
        string? referenceEntityType = null,
        CancellationToken cancellationToken = default)
    {
        var notification = new AppNotification(
            Guid.NewGuid(),
            userId,
            type,
            message,
            actorUserId,
            referenceEntityId,
            referenceEntityType);

        await _notificationRepository.AddAsync(notification, cancellationToken);
    }
}
