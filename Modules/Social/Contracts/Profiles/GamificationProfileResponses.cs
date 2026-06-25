namespace backend.Modules.Social.Contracts.Profiles;

public record PublicGoalResponse(Guid Id, string TemplateName, double TargetValue, double CurrentValue, string Unit, string Status, bool IsRecurring, DateTime CreatedAt);
public record PublicBadgeResponse(Guid BadgeId, string Code, string Name, string? Description, string Tier, DateTime AwardedAt, int? FeaturedOrder);
public record PublicChallengeResultResponse(Guid ChallengeId, string ChallengeTitle, string Mode, double Score, int Rank, bool IsFinisher, bool IsWinner, DateTime FinalizedAt);
