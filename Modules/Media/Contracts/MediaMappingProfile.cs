using AutoMapper;
using backend.Modules.Media.Domain;

namespace backend.Modules.Media.Contracts;

public class MediaMappingProfile : Profile
{
    public MediaMappingProfile()
    {
        CreateMap<MediaVariant, MediaVariantResponse>();
        CreateMap<MediaAsset, MediaAssetStatusResponse>()
            .ForCtorParam(nameof(MediaAssetStatusResponse.Variants), options => options.MapFrom(asset => asset.Variants));
    }
}
