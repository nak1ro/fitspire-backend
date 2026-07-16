using AutoMapper;
using backend.Modules.BodyTracking.Contracts;
using backend.Modules.BodyTracking.Domain;

namespace backend.Modules.BodyTracking.Mappings;

public class BodyCheckInMappingProfile : Profile
{
    public BodyCheckInMappingProfile()
    {
        CreateMap<BodyCheckIn, BodyCheckInResponse>()
            .ForCtorParam(nameof(BodyCheckInResponse.Photo), options => options.MapFrom(_ => (backend.Modules.Media.Contracts.MediaResponse?)null));
    }
}
