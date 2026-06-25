using AutoMapper;
using backend.Modules.Shared.Constants;
using backend.Modules.Shared.Service;
using backend.Modules.User.Domain;
using backend.Modules.User.DTOs;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace backend.Modules.User.Services;

public class UserService : IUserService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IBlobService _blobService;
    private readonly IMapper _mapper;

    public UserService(UserManager<AppUser> userManager, IBlobService blobService, IMapper mapper)
    {
        _userManager = userManager;
        _blobService = blobService;
        _mapper = mapper;
    }

    private async Task<AppUser> GetUserOrThrowAsync(Guid userId)
    {
        var user = await _userManager.Users
            .Include(u => u.AppUserPreference)
            .FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
            throw new UnauthorizedAccessException("User not found.");
        return user;
    }

    private static void ValidateProfilePicture(IFormFile file)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("File is required.");
        if (file.Length > FileUploadConstants.MaxProfilePictureSize)
            throw new ArgumentException("File is too large. Maximum size is 5 MB.");
        if (!FileUploadConstants.AllowedProfilePictureTypes.Contains(file.ContentType.ToLower()))
            throw new ArgumentException("Unsupported file type. Only JPEG, PNG, or WebP images are allowed.");

        var extension = Path.GetExtension(file.FileName).ToLower();
        if (!FileUploadConstants.AllowedProfilePictureExtensions.Contains(extension))
            throw new ArgumentException("Unsupported file extension. Only .jpg, .jpeg, .png, .webp are allowed.");
    }

    public async Task<UserProfileDto> GetProfileAsync(Guid userId)
    {
        var user = await GetUserOrThrowAsync(userId);
        return _mapper.Map<UserProfileDto>(user);
    }

    public async Task<UserPreferencesDto> GetPreferencesAsync(Guid userId)
    {
        var user = await GetUserOrThrowAsync(userId);
        var preferences = user.AppUserPreference ?? new AppUserPreference();
        return _mapper.Map<UserPreferencesDto>(preferences);
    }

    public async Task<UserProfileDto> UpdateProfileAsync(Guid userId, UpdateProfileDto dto)
    {
        var user = await GetUserOrThrowAsync(userId);

        if (dto.DisplayName != null)
            user.DisplayName = dto.DisplayName;
        if (dto.Bio != null)
            user.Bio = dto.Bio;
        if (dto.IsPrivate.HasValue)
            user.IsPrivate = dto.IsPrivate.Value;

        user.UpdatedAt = DateTime.UtcNow;
        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
            throw new InvalidOperationException(string.Join("; ", result.Errors.Select(e => e.Description)));

        return _mapper.Map<UserProfileDto>(user);
    }

    public async Task<UserProfileDto> UpdateProfilePictureAsync(Guid userId, IFormFile file)
    {
        var user = await GetUserOrThrowAsync(userId);
        ValidateProfilePicture(file);

        var fileName = $"{user.Id}_{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        await using var stream = file.OpenReadStream();

        string url;
        try
        {
            url = await _blobService.UploadFileAsync(stream, fileName, file.ContentType);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("File upload failed, please try again.", ex);
        }

        user.ProfilePictureUrl = url;
        user.UpdatedAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        return _mapper.Map<UserProfileDto>(user);
    }

    public async Task<UserPreferencesDto> UpdatePreferencesAsync(Guid userId, UpdateUserPreferencesDto dto)
    {
        var user = await GetUserOrThrowAsync(userId);

        var prefs = user.AppUserPreference ?? new AppUserPreference
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            CreatedAt = DateTime.UtcNow
        };

        if (dto.PreferredLanguage != null)
            prefs.PreferredLanguage = dto.PreferredLanguage;
        if (dto.IsDarkModeEnabled.HasValue)
            prefs.IsDarkModeEnabled = dto.IsDarkModeEnabled.Value;
        if (dto.ReceiveEmailNotifications.HasValue)
            prefs.ReceiveEmailNotifications = dto.ReceiveEmailNotifications.Value;
        if (dto.UnitSystem != null)
            prefs.UnitSystem = dto.UnitSystem;
        if (dto.TimeZoneId != null)
            prefs.TimeZoneId = dto.TimeZoneId;

        prefs.UpdatedAt = DateTime.UtcNow;
        user.AppUserPreference = prefs;
        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
            throw new InvalidOperationException(string.Join("; ", result.Errors.Select(e => e.Description)));

        return _mapper.Map<UserPreferencesDto>(prefs);
    }
}
