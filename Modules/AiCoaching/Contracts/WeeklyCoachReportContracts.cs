using backend.Modules.AiCoaching.Domain;

namespace backend.Modules.AiCoaching.Contracts;

public sealed record GenerateWeeklyCoachReportRequest(DateOnly? PeriodStart = null);

public sealed record WeeklyCoachReportHistoryFilter(int Page = 1, int PageSize = 20);

public sealed record WeeklyCoachReportListItemResponse(
    Guid Id,
    DateOnly PeriodStart,
    DateOnly PeriodEnd,
    WeeklyCoachReportStatus Status,
    bool HasReportContent,
    bool CanRetry,
    int GenerationCount,
    DateTime RequestedAt,
    DateTime? CompletedAt,
    DateTime? FailedAt);

public sealed record WeeklyCoachReportPageResponse(
    IReadOnlyList<WeeklyCoachReportListItemResponse> Items,
    int Page,
    int PageSize,
    int TotalCount);

public sealed record WeeklyCoachReportResponse(
    Guid Id,
    DateOnly PeriodStart,
    DateOnly PeriodEnd,
    WeeklyCoachReportStatus Status,
    bool HasReportContent,
    bool CanRetry,
    int GenerationCount,
    DateTime RequestedAt,
    DateTime? ProcessingStartedAt,
    DateTime? CompletedAt,
    DateTime? FailedAt,
    string? FailureMessage,
    WeeklyCoachCoverageResponse Coverage,
    WeeklyCoachReportContentResponse? Content,
    string WellnessDisclaimer);

public sealed record WeeklyCoachCoverageResponse(
    WeeklyCoachSectionCoverageResponse Workouts,
    WeeklyCoachSectionCoverageResponse Goals,
    WeeklyCoachSectionCoverageResponse Challenges,
    WeeklyCoachSectionCoverageResponse Body,
    WeeklyCoachSectionCoverageResponse Nutrition);

public sealed record WeeklyCoachSectionCoverageResponse(string State, int RecordCount);

public sealed record WeeklyCoachReportContentResponse(
    string Headline,
    string Overview,
    IReadOnlyList<WeeklyCoachObservationResponse> Wins,
    IReadOnlyList<WeeklyCoachObservationResponse> Patterns,
    IReadOnlyList<WeeklyCoachActionResponse> NextWeekActions,
    IReadOnlyList<string> DataLimitations);

public sealed record WeeklyCoachObservationResponse(
    string Title,
    string Explanation,
    string Category,
    IReadOnlyList<string> EvidenceKeys);

public sealed record WeeklyCoachActionResponse(
    string Title,
    string Explanation,
    string Category,
    IReadOnlyList<string> EvidenceKeys);

public sealed record WeeklyCoachGenerationResponse(WeeklyCoachReportResponse Report, bool Accepted);
