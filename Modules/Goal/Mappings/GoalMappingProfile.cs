using AutoMapper;
using backend.Modules.Goal.Domain.Entities;
using backend.Modules.Goal.DTOs;
using backend.Modules.Goal.Services;

namespace backend.Modules.Goal.Mappings;

public class GoalMappingProfile : Profile
{
    public GoalMappingProfile()
    {
        CreateMap<GoalPeriod, GoalPeriodResponse>();
        CreateMap<GoalProgressEntry, GoalProgressEntryResponse>();
        CreateMap<GoalType, GoalTypeResponse>()
            .ForCtorParam(nameof(GoalTypeResponse.AllowedSchedules), options =>
                options.MapFrom(template => GoalTemplatePolicy.GetAllowedSchedules(template)));
    }
}
