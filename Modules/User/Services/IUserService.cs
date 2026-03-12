using backend.Modules.User.DTOs;

namespace backend.Modules.User.Services;

public interface IUserService
{
    Task<UserProfileDto> GetProfileAsync();
    Task<UserPreferencesDto> GetPreferencesAsync();
    Task<UserProfileDto> UpdateProfileAsync(UpdateProfileDto dto);
    Task<UserPreferencesDto> UpdatePreferencesAsync(UpdateUserPreferencesDto dto);
    Task<UserProfileDto> UpdateProfilePictureAsync(IFormFile file);
}
