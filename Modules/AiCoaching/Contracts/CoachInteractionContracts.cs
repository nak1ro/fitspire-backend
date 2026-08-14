using backend.Modules.AiCoaching.Domain;

namespace backend.Modules.AiCoaching.Contracts;

public sealed record CreateCoachThreadRequest(string? Title = null);
public sealed record UpdateCoachThreadRequest(string Title);
public sealed record CoachThreadHistoryFilter(int Page = 1, int PageSize = 20);
public sealed record SendCoachMessageRequest(Guid ClientRequestId, string Content);
public sealed record CoachMessageHistoryFilter(int? BeforeSequence = null, int PageSize = 20);

public sealed record CoachThreadListItemResponse(Guid Id, string Title, int MessageCount, DateTime CreatedAt,
    DateTime LastActivityAt);
public sealed record CoachThreadPageResponse(IReadOnlyList<CoachThreadListItemResponse> Items, int Page, int PageSize,
    int TotalCount);
public sealed record CoachThreadResponse(Guid Id, string Title, int MessageCount, DateTime CreatedAt,
    DateTime LastActivityAt);

public sealed record CoachQueuedExchangeResponse(CoachMessageResponse UserMessage, CoachMessageResponse AssistantMessage,
    bool Accepted);
public sealed record CoachMessageHistoryResponse(IReadOnlyList<CoachMessageResponse> Items, int? NextBeforeSequence);
public sealed record CoachMessageResponse(Guid Id, int SequenceNumber, CoachMessageRole Role, CoachGenerationStatus Status,
    string? Content, CoachAnswerContentResponse? Answer, DateTime RequestedAt, DateTime? ProcessingStartedAt,
    DateTime? CompletedAt, DateTime? FailedAt, string? FailureMessage, bool CanRetry);
public sealed record CoachAnswerContentResponse(string AnswerMarkdown, IReadOnlyList<CoachSuggestedActionResponse> SuggestedActions,
    IReadOnlyList<string> DataLimitations, string SafetyCategory, string WellnessDisclaimer);
public sealed record CoachSuggestedActionResponse(string Title, string Description, string Category);

public sealed record DailyCoachBriefingResponse(Guid Id, DateOnly LocalDate, CoachGenerationStatus Status,
    DateTime RequestedAt, DateTime? ProcessingStartedAt, DateTime? CompletedAt, DateTime? FailedAt,
    string? FailureMessage, bool CanRetry, DailyCoachBriefingContentResponse? Content, string WellnessDisclaimer);
public sealed record DailyCoachBriefingContentResponse(string Headline, string Focus, string SummaryMarkdown,
    CoachSuggestedActionResponse NextAction, string InsightMarkdown, IReadOnlyList<string> DataLimitations);
