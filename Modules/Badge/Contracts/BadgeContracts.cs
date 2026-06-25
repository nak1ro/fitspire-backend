namespace backend.Modules.Badge.Contracts;

public record BadgePageResponse<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount);

public record BadgeCatalogueFilter(string? Category = null, bool? Earned = null, int Page = 1, int PageSize = 20);

public record BadgeCollectionFilter(string? Category = null, bool? Featured = null, int Page = 1, int PageSize = 20);

public record PublicBadgeFilter(string? Category = null, int Page = 1, int PageSize = 20);

public record SetFeaturedBadgesRequest(IReadOnlyList<Guid> BadgeIds);

public record BadgeDefinitionResponse(
    Guid BadgeId,
    string Code,
    string Name,
    string? Description,
    string? IconUrl,
    string Category,
    string? SeriesCode,
    string Tier,
    string CriterionCode,
    double Threshold,
    string CanonicalUnit);

public record BadgeEvidenceResponse(
    string? CriterionCode,
    double? Threshold,
    double? AchievedValue,
    string? CanonicalUnit,
    string? TriggeringEntityType,
    Guid? TriggeringEntityId,
    string? Summary);

public record BadgeCatalogueItemResponse(
    BadgeDefinitionResponse Badge,
    bool IsEarned,
    DateTime? AwardedAt,
    int? FeaturedOrder,
    double? CurrentProgress,
    double? ProgressPercentage,
    BadgeEvidenceResponse? Evidence);

public record EarnedBadgeResponse(
    BadgeDefinitionResponse Badge,
    DateTime AwardedAt,
    int? FeaturedOrder,
    BadgeEvidenceResponse Evidence);

public record PublicBadgeEvidenceResponse(
    string? CriterionCode,
    double? Threshold,
    double? AchievedValue,
    string? CanonicalUnit,
    string? Summary);

public record PublicBadgeResponse(
    Guid BadgeId,
    string Code,
    string Name,
    string? Description,
    string? IconUrl,
    string Category,
    string? SeriesCode,
    string Tier,
    DateTime AwardedAt,
    int? FeaturedOrder,
    PublicBadgeEvidenceResponse Evidence);
