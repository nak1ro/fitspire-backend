namespace backend.Modules.Moderation.Contracts;

public interface IAdminModerationService
{
    Task<AdminModerationQueueSummaryResponse> GetSummaryAsync(CancellationToken cancellationToken = default);
    Task<AdminModerationReportPageResponse> GetReportsAsync(AdminModerationReportFilter filter, CancellationToken cancellationToken = default);
    Task<AdminModerationReportDetailResponse> GetReportAsync(Guid reportId, CancellationToken cancellationToken = default);
    Task<AdminModerationReportDetailResponse> ResolveAsync(Guid moderatorUserId, Guid reportId,
        ResolveModerationReportRequest request, CancellationToken cancellationToken = default);
    Task<AdminModerationReportDetailResponse> RestoreTargetAsync(Guid moderatorUserId, Guid reportId,
        CancellationToken cancellationToken = default);
    Task<AdminModerationReportDetailResponse> UnsuspendUserAsync(Guid moderatorUserId, Guid reportId,
        CancellationToken cancellationToken = default);
}
