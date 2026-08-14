using AutoMapper;
using backend.Modules.AiCoaching.Domain;

namespace backend.Modules.AiCoaching.Contracts;

public sealed class CoachInteractionMappingProfile : Profile
{
    public CoachInteractionMappingProfile()
    {
        CreateMap<CoachThread, CoachThreadListItemResponse>()
            .ForCtorParam(nameof(CoachThreadListItemResponse.MessageCount), options => options.MapFrom(thread => thread.NextSequenceNumber));
        CreateMap<CoachThread, CoachThreadResponse>()
            .ForCtorParam(nameof(CoachThreadResponse.MessageCount), options => options.MapFrom(thread => thread.NextSequenceNumber));
    }
}
