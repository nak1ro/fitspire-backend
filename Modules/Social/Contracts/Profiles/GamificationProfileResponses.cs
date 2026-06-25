namespace backend.Modules.Social.Contracts.Profiles;

public record PublicGoalResponse(Guid Id, string TemplateName, double TargetValue, double CurrentValue, string Unit, string Status, bool IsRecurring, DateTime CreatedAt);
public record PublicGoalPeriodResponse(Guid GoalId, string TemplateName, DateTime StartAt, DateTime EndAt,
    double TargetValue, double ProgressValue, DateTime CompletedAt);
public record PublicChallengeResultResponse(Guid ChallengeId, string ChallengeTitle, string Mode, double Score, int Rank, bool IsFinisher, bool IsWinner, DateTime FinalizedAt);
