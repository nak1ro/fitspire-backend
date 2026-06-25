using backend.Modules.Shared.Domain;
using backend.Modules.User.Domain;

namespace backend.Modules.Badge.Domain;

public class UserBadge
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid BadgeId { get; private set; }
    public DateTime AwardedAt { get; private set; }
    public double? AchievedValue { get; private set; }
    public string? CriterionCode { get; private set; }
    public double? ThresholdSnapshot { get; private set; }
    public string? CanonicalUnit { get; private set; }
    public string? TriggeringEntityType { get; private set; }
    public Guid? TriggeringEntityId { get; private set; }
    public string? EvidenceSummary { get; private set; }
    public int? FeaturedOrder { get; private set; }

    public AppUser User { get; private set; } = null!;
    public AchievementBadge AchievementBadge { get; private set; } = null!;

    private UserBadge() { }

    private UserBadge(Guid userId, AchievementBadge badge, BadgeAwardEvidence evidence, DateTime awardedAt)
    {
        if (userId == Guid.Empty)
            throw new DomainException("A badge award requires a user.");
        if (badge.Id == Guid.Empty)
            throw new DomainException("A badge award requires a persisted badge definition.");

        evidence.EnsureValid();
        Id = Guid.NewGuid();
        UserId = userId;
        BadgeId = badge.Id;
        AchievementBadge = badge;
        AwardedAt = awardedAt;
        AchievedValue = evidence.AchievedValue;
        CriterionCode = evidence.CriterionCode;
        ThresholdSnapshot = evidence.Threshold;
        CanonicalUnit = evidence.CanonicalUnit;
        TriggeringEntityType = NormalizeOptional(evidence.TriggeringEntityType);
        TriggeringEntityId = evidence.TriggeringEntityId;
        EvidenceSummary = NormalizeOptional(evidence.Summary);
    }

    public static UserBadge Award(Guid userId, AchievementBadge badge, BadgeAwardEvidence evidence, DateTime? awardedAt = null) =>
        new(userId, badge, evidence, awardedAt ?? DateTime.UtcNow);

    public void SetFeaturedOrder(int order)
    {
        if (order is < 1 or > 5)
            throw new DomainException("Featured badge order must be between 1 and 5.");

        FeaturedOrder = order;
    }

    public void ClearFeaturedOrder()
    {
        FeaturedOrder = null;
    }

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

public sealed record BadgeAwardEvidence(
    string CriterionCode,
    double Threshold,
    double AchievedValue,
    string CanonicalUnit,
    string? TriggeringEntityType = null,
    Guid? TriggeringEntityId = null,
    string? Summary = null)
{
    public void EnsureValid()
    {
        if (string.IsNullOrWhiteSpace(CriterionCode))
            throw new DomainException("Badge evidence requires a criterion code.");
        if (Threshold <= 0 || double.IsNaN(Threshold) || double.IsInfinity(Threshold))
            throw new DomainException("Badge evidence threshold must be finite and greater than zero.");
        if (AchievedValue < 0 || double.IsNaN(AchievedValue) || double.IsInfinity(AchievedValue))
            throw new DomainException("Badge evidence achieved value must be finite and non-negative.");
        if (string.IsNullOrWhiteSpace(CanonicalUnit))
            throw new DomainException("Badge evidence requires a canonical unit.");
        if (Summary?.Length > 500)
            throw new DomainException("Badge evidence summary must be at most 500 characters.");
    }
}
