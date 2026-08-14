using backend.Modules.Moderation.Domain;

namespace backend.Modules.Moderation.Contracts;

public sealed record CreateModerationReportRequest(
    ModerationReportTargetType TargetType,
    Guid TargetId,
    ModerationReportReason Reason,
    string? Details);

public sealed record ModerationReportSubmissionResponse(
    Guid Id,
    ModerationReportStatus Status,
    DateTime CreatedAt,
    bool AlreadyReported);

public sealed record AdminModerationReportFilter(
    ModerationReportStatus? Status = ModerationReportStatus.Open,
    ModerationReportTargetType? TargetType = null,
    ModerationReportReason? Reason = null,
    int Page = 1,
    int PageSize = 20);

public sealed record AdminModerationReportPageResponse(
    IReadOnlyList<AdminModerationReportListItemResponse> Items,
    int Page,
    int PageSize,
    int TotalCount);

public sealed record AdminModerationReportListItemResponse(
    Guid Id,
    ModerationReportStatus Status,
    ModerationReportTargetType TargetType,
    ModerationReportReason Reason,
    DateTime CreatedAt,
    AdminModerationUserResponse Reporter,
    AdminModerationUserResponse Subject,
    bool IsTargetCurrentlyRemoved);

public sealed record AdminModerationUserResponse(Guid Id, string UserName, string DisplayName);

public sealed record AdminModerationQueueSummaryResponse(int OpenReports, int ResolvedReports);

public sealed record AdminModerationTargetResponse(
    bool Exists,
    bool IsRemoved,
    string? Content,
    Guid? PostId,
    AdminModerationUserResponse? Profile,
    backend.Modules.Media.Contracts.MediaResponse? Media);

public sealed record AdminModerationActionResponse(
    Guid Id,
    ModerationActionType ActionType,
    DateTime OccurredAt,
    string? Note,
    DateTime? SuspensionEndsAt,
    AdminModerationUserResponse Moderator);

public sealed record AdminModerationReportDetailResponse(
    Guid Id,
    ModerationReportStatus Status,
    ModerationResolutionOutcome? ResolutionOutcome,
    ModerationReportTargetType TargetType,
    Guid TargetId,
    ModerationMediaContext? MediaContext,
    ModerationReportReason Reason,
    string? Details,
    DateTime CreatedAt,
    AdminModerationUserResponse Reporter,
    AdminModerationUserResponse Subject,
    string TargetSnapshotJson,
    AdminModerationTargetResponse CurrentTarget,
    DateTime? SubjectSuspendedUntil,
    IReadOnlyList<AdminModerationActionResponse> Actions);

public sealed record ResolveModerationReportRequest(
    AdminModerationResolutionAction Action,
    string? ModeratorNote,
    int? SuspensionDurationDays);
