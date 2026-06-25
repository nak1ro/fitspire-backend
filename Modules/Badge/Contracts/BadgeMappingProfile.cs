using AutoMapper;
using backend.Modules.Badge.Domain;

namespace backend.Modules.Badge.Contracts;

public class BadgeMappingProfile : Profile
{
    public BadgeMappingProfile()
    {
        CreateMap<AchievementBadge, BadgeDefinitionResponse>()
            .ForCtorParam(nameof(BadgeDefinitionResponse.BadgeId), options => options.MapFrom(badge => badge.Id));
    }
}
