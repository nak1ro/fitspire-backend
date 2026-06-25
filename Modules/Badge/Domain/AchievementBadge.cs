using backend.Modules.Badge.Domain.Constants;
using backend.Modules.Shared.Domain;

namespace backend.Modules.Badge.Domain;

public class AchievementBadge
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public string? IconUrl { get; private set; }
    public string Code { get; private set; } = null!;
    public string Category { get; private set; } = null!;
    public string? SeriesCode { get; private set; }
    public string Tier { get; private set; } = null!;
    public string CriterionCode { get; private set; } = null!;
    public double Threshold { get; private set; }
    public string? MetricCode { get; private set; }
    public string CanonicalUnit { get; private set; } = null!;
    public int DisplayOrder { get; private set; }
    public bool ShowProgressWhenLocked { get; private set; }
    public bool IsActive { get; private set; }

    public ICollection<UserBadge> UserBadges { get; private set; } = new List<UserBadge>();

    private AchievementBadge() { }

    private AchievementBadge(BadgeDefinition definition)
    {
        Id = Guid.NewGuid();
        Apply(definition);
        IsActive = true;
    }

    public static AchievementBadge Create(BadgeDefinition definition) => new(definition);

    public void Synchronize(BadgeDefinition definition)
    {
        Apply(definition);
        IsActive = true;
    }

    public void Retire()
    {
        IsActive = false;
    }

    private void Apply(BadgeDefinition definition)
    {
        definition.EnsureValid();

        Name = definition.Name.Trim();
        Description = NormalizeOptional(definition.Description);
        IconUrl = NormalizeOptional(definition.IconUrl);
        Code = definition.Code.Trim().ToLowerInvariant();
        Category = definition.Category;
        SeriesCode = NormalizeOptional(definition.SeriesCode)?.ToLowerInvariant();
        Tier = definition.Tier;
        CriterionCode = definition.CriterionCode;
        Threshold = definition.Threshold;
        MetricCode = NormalizeOptional(definition.MetricCode);
        CanonicalUnit = definition.CanonicalUnit;
        DisplayOrder = definition.DisplayOrder;
        ShowProgressWhenLocked = definition.ShowProgressWhenLocked;
    }

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

public sealed record BadgeDefinition(
    string Code,
    string Name,
    string? Description,
    string? IconUrl,
    string Category,
    string? SeriesCode,
    string Tier,
    string CriterionCode,
    double Threshold,
    string? MetricCode,
    string CanonicalUnit,
    int DisplayOrder,
    bool ShowProgressWhenLocked = true)
{
    public void EnsureValid()
    {
        if (string.IsNullOrWhiteSpace(Code) || Code.Length > 80)
            throw new DomainException("Badge code is required and must be at most 80 characters.");
        if (string.IsNullOrWhiteSpace(Name) || Name.Length > 160)
            throw new DomainException("Badge name is required and must be at most 160 characters.");
        if (!BadgeCategories.IsKnown(Category))
            throw new DomainException($"Unsupported badge category '{Category}'.");
        if (!BadgeTiers.IsKnown(Tier))
            throw new DomainException($"Unsupported badge tier '{Tier}'.");
        if (!BadgeCriterionCodes.IsKnown(CriterionCode))
            throw new DomainException($"Unsupported badge criterion '{CriterionCode}'.");
        if (Threshold <= 0 || double.IsNaN(Threshold) || double.IsInfinity(Threshold))
            throw new DomainException("Badge threshold must be a finite value greater than zero.");
        if (string.IsNullOrWhiteSpace(CanonicalUnit) || CanonicalUnit.Length > 32)
            throw new DomainException("Badge canonical unit is required and must be at most 32 characters.");
        if (DisplayOrder <= 0)
            throw new DomainException("Badge display order must be greater than zero.");
    }
}
