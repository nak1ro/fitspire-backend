using backend.Modules.User.DTOs;

namespace backend.Modules.User.Services;

public interface IUserService
{
    Task<UserProfileDto> GetProfileAsync(Guid userId);
    Task<UserPreferencesDto> GetPreferencesAsync(Guid userId);
    Task<UserProfileDto> UpdateProfileAsync(Guid userId, UpdateProfileDto dto);
    Task<UserPreferencesDto> UpdatePreferencesAsync(Guid userId, UpdateUserPreferencesDto dto);
    Task<UserProfileDto> UpdateProfilePictureAsync(Guid userId, IFormFile file);
}
