using AutoMapper;
using backend.Modules.User.Domain;
using backend.Modules.User.DTOs;

namespace backend.Modules.User.Mappings;

public class UserProfileMapping : Profile
{
    public UserProfileMapping()
    {
        CreateMap<AppUser, UserProfileDto>();
        CreateMap<AppUserPreference, UserPreferencesDto>();
    }
}