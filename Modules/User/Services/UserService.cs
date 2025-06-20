using System.Security.Claims;
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
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IBlobService _blobService;
    private readonly IMapper _mapper;

    public UserService(UserManager<AppUser> userManager, IHttpContextAccessor httpContextAccessor,
        IBlobService blobService, IMapper mapper)
    {
        _userManager = userManager;
        _httpContextAccessor = httpContextAccessor;
        _blobService = blobService;
        _mapper = mapper;
    }

    private string GetCurrentUserIdOrThrow()
    {
        var userId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            throw new UnauthorizedAccessException("User not found.");
        return userId;
    }

    private async Task<AppUser> GetCurrentUserOrThrowAsync()
    {
        var userId = GetCurrentUserIdOrThrow();
        var user = await _userManager.Users
            .Include(u => u.AppUserPreference)
            .FirstOrDefaultAsync(u => u.Id == Guid.Parse(userId));
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
        if (!FileUploadConstants.AllowedProfilePictureTypes.Contains(extension))
            throw new ArgumentException("Unsupported file extension. Only .jpg, .jpeg, .png, .webp are allowed.");
    }

    public async Task<UserProfileDto> UpdateProfileAsync(UpdateProfileDto dto)
    {
        var user = await GetCurrentUserOrThrowAsync();

        if (dto.DisplayName != null)
            user.DisplayName = dto.DisplayName;
        if (dto.Bio != null)
            user.Bio = dto.Bio;

        user.UpdatedAt = DateTime.UtcNow;
        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
            throw new InvalidOperationException(string.Join("; ", result.Errors.Select(e => e.Description)));

        return _mapper.Map<UserProfileDto>(user);
    }

    public async Task<UserProfileDto> UpdateProfilePictureAsync(IFormFile file)
    {
        var user = await GetCurrentUserOrThrowAsync();
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

    public async Task<UserPreferencesDto> UpdatePreferencesAsync(UpdateUserPreferencesDto dto)
    {
        var user = await GetCurrentUserOrThrowAsync();

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

        prefs.UpdatedAt = DateTime.UtcNow;
        user.AppUserPreference = prefs;
        await _userManager.UpdateAsync(user);

        return _mapper.Map<UserPreferencesDto>(prefs);
    }
}