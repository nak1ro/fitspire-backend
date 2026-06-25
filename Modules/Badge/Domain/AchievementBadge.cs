namespace backend.Modules.Badge.Domain;

public class AchievementBadge
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string? IconUrl { get; set; }
    public string Code { get; set; } = null!;
    public string Category { get; set; } = "Fitness";
    public string? SeriesCode { get; set; }
    public string Tier { get; set; } = "None";
    public string CriterionCode { get; set; } = null!;
    public double Threshold { get; set; }
    public string? MetricCode { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<UserBadge> UserBadges { get; set; } = new List<UserBadge>();
}
