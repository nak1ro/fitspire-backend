using backend.Modules.User.Domain;

namespace backend.Modules.Notification.Domain;

public class AppNotification
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }
    public string Message { get; set; } = null!;
    public bool IsRead { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public AppUser User { get; set; } = null!;
}