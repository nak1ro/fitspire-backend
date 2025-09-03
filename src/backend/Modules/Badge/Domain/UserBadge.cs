using backend.Modules.User.Domain;

namespace backend.Modules.Badge.Domain;

public class UserBadge
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid BadgeId { get; set; }
    public DateTime AwardedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public AppUser User { get; set; } = null!;
    public AchievementBadge AchievementBadge { get; set; } = null!;
}