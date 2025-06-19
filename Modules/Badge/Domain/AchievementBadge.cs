namespace backend.Modules.Badge.Domain;

public class AchievementBadge
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string? IconUrl { get; set; }

    public ICollection<UserBadge> UserBadges { get; set; } = new List<UserBadge>();
}