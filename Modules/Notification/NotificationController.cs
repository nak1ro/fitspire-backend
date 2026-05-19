using backend.Modules.Notification.DTOs;
using backend.Modules.Notification.Services;
using backend.Modules.Shared.Domain;
using backend.Modules.Shared.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Modules.Notification;

[ApiController]
[Route("api/notifications")]
[Authorize]
public class NotificationController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<NotificationResponse>>> GetNotifications(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        ValidatePagination(page, pageSize);

        var userId = User.GetRequiredUserId();
        var notifications = await _notificationService.GetNotificationsAsync(
            userId,
            page,
            pageSize,
            cancellationToken);

        return Ok(notifications);
    }

    [HttpGet("unread-count")]
    public async Task<ActionResult<UnreadNotificationCountResponse>> GetUnreadCount(
        CancellationToken cancellationToken = default)
    {
        var userId = User.GetRequiredUserId();
        var count = await _notificationService.GetUnreadCountAsync(userId, cancellationToken);
        return Ok(new UnreadNotificationCountResponse(count));
    }

    [HttpPost("{id:guid}/read")]
    public async Task<IActionResult> MarkRead(Guid id, CancellationToken cancellationToken = default)
    {
        var userId = User.GetRequiredUserId();
        await _notificationService.MarkReadAsync(userId, id, cancellationToken);
        return NoContent();
    }

    [HttpPost("read-all")]
    public async Task<ActionResult<MarkAllNotificationsReadResponse>> MarkAllRead(
        CancellationToken cancellationToken = default)
    {
        var userId = User.GetRequiredUserId();
        var count = await _notificationService.MarkAllReadAsync(userId, cancellationToken);
        return Ok(new MarkAllNotificationsReadResponse(count));
    }

    private static void ValidatePagination(int page, int pageSize)
    {
        if (page < 1)
            throw new DomainException("Page must be greater than zero.");

        if (pageSize is < 1 or > 100)
            throw new DomainException("Page size must be between 1 and 100.");
    }
}
