using AutoMapper;
using backend.Modules.Social.Contracts.Profiles;
using backend.Modules.User.Domain;

namespace backend.Modules.Social.Mappings;

public class SocialMappingProfile : Profile
{
    public SocialMappingProfile()
    {
        CreateMap<AppUser, SocialUserSummaryResponse>()
            .ForCtorParam(nameof(SocialUserSummaryResponse.UserName), options =>
                options.MapFrom(user => user.UserName ?? string.Empty));
    }
}
