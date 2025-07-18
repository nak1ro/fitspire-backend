using System.Security.Claims;
using AutoMapper;
using backend.Modules.Shared.Constants;
using backend.Modules.Shared.Service;
using backend.Modules.User.Domain;
using backend.Modules.User.DTOs;
using backend.Modules.User.Repositories;
using Microsoft.AspNetCore.Identity;

namespace backend.Modules.User.Services;

public class UserService : IUserService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IBlobService _blobService;
    private readonly IMapper _mapper;
    private readonly IUserRepository _userRepository;

    public UserService(
        UserManager<AppUser> userManager,
        IHttpContextAccessor httpContextAccessor,
        IBlobService blobService,
        IMapper mapper,
        IUserRepository userRepository)
    {
        _userManager = userManager;
        _httpContextAccessor = httpContextAccessor;
        _blobService = blobService;
        _mapper = mapper;
        _userRepository = userRepository;
    }

    private string GetCurrentUserIdOrThrow()
    {
        var userId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) throw new UnauthorizedAccessException("User not found.");
        return userId;
    }

    private async Task<AppUser> GetCurrentUserOrThrowAsync()
    {
        var userId = GetCurrentUserIdOrThrow();
        var guid = Guid.Parse(userId);

        var user = await _userRepository.GetByIdWithPrefsAsync(guid);
        if (user == null) throw new UnauthorizedAccessException("User not found.");
        return user;
    }

    // === PROFILE ===
    public async Task<UserProfileDto> GetProfileAsync()
    {
        var user = await GetCurrentUserOrThrowAsync();
        return _mapper.Map<UserProfileDto>(user);
    }

    public async Task<UserProfileDto> UpdateProfileAsync(UpdateProfileDto dto)
    {
        var user = await GetCurrentUserOrThrowAsync();

        if (dto.DisplayName != null) user.DisplayName = dto.DisplayName;
        if (dto.Bio != null) user.Bio = dto.Bio;
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

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
            throw new InvalidOperationException(string.Join("; ", result.Errors.Select(e => e.Description)));

        return _mapper.Map<UserProfileDto>(user);
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
        if (!(extension == ".jpg" || extension == ".jpeg" || extension == ".png" || extension == ".webp"))
            throw new ArgumentException("Unsupported file extension. Only .jpg, .jpeg, .png, .webp are allowed.");
    }

    public async Task<UserPreferencesDto> GetPreferencesAsync()
    {
        var user = await GetCurrentUserOrThrowAsync();

        var prefs = await _userRepository.EnsurePreferencesAsync(user.Id);

        user.AppUserPreference = prefs;

        return _mapper.Map<UserPreferencesDto>(prefs);
    }

    public async Task<UserProfileDto> GetUserByUsernameAsync(string username)
    {
        var user = await _userRepository.GetByUsernameAsync(username);
        if (user == null)
            throw new KeyNotFoundException($"User with username '{username}' not found.");

        return _mapper.Map<UserProfileDto>(user);
    }

    public async Task<UserPreferencesDto> UpdatePreferencesAsync(UpdateUserPreferencesDto dto)
    {
        var user = await GetCurrentUserOrThrowAsync();

        var updated = await _userRepository.UpsertPreferencesAsync(user.Id, prefs =>
        {
            if (dto.PreferredLanguage != null) prefs.PreferredLanguage = dto.PreferredLanguage;
            if (dto.IsDarkModeEnabled.HasValue) prefs.IsDarkModeEnabled = dto.IsDarkModeEnabled.Value;
            if (dto.ReceiveEmailNotifications.HasValue)
                prefs.ReceiveEmailNotifications = dto.ReceiveEmailNotifications.Value;
            if (dto.UnitSystem != null) prefs.UnitSystem = dto.UnitSystem;
        });

        // keep navigation consistent
        user.AppUserPreference = updated;

        return _mapper.Map<UserPreferencesDto>(updated);
    }
}