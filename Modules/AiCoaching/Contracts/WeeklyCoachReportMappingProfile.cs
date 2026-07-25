using AutoMapper;
using backend.Modules.AiCoaching.Domain;

namespace backend.Modules.AiCoaching.Contracts;

public sealed class WeeklyCoachReportMappingProfile : Profile
{
    public WeeklyCoachReportMappingProfile()
    {
        CreateMap<WeeklyCoachReport, WeeklyCoachReportListItemResponse>()
            .ForCtorParam(nameof(WeeklyCoachReportListItemResponse.HasReportContent),
                options => options.MapFrom(report => report.HasDisplayableContent))
            .ForCtorParam(nameof(WeeklyCoachReportListItemResponse.CanRetry),
                options => options.MapFrom(report => report.Status == WeeklyCoachReportStatus.Failed));
    }
}
