using backend.Modules.AiCoaching.Contracts;

namespace backend.Modules.AiCoaching.Services;

public interface IWeeklyCoachReportService
{
    Task<WeeklyCoachGenerationResponse> RequestGenerationAsync(Guid userId, GenerateWeeklyCoachReportRequest request,
        CancellationToken cancellationToken);
    Task<WeeklyCoachReportResponse> GetAsync(Guid userId, Guid reportId, CancellationToken cancellationToken);
    Task<WeeklyCoachReportPageResponse> GetHistoryAsync(Guid userId, WeeklyCoachReportHistoryFilter filter,
        CancellationToken cancellationToken);
    Task DeleteAsync(Guid userId, Guid reportId, CancellationToken cancellationToken);
}
