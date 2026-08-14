namespace backend.Modules.Moderation.Contracts;

public interface IModerationReportService
{
    Task<ModerationReportSubmissionResponse> CreateAsync(
        Guid reporterUserId,
        CreateModerationReportRequest request,
        CancellationToken cancellationToken = default);
}
