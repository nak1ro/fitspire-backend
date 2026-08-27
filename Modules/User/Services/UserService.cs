using AutoMapper;
using backend.Data;
using backend.Modules.Media.Domain;
using backend.Modules.Media.Contracts;
using backend.Modules.User.Domain;
using backend.Modules.User.Domain.Constants;
using backend.Modules.User.DTOs;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace backend.Modules.User.Services;

public class UserService : IUserService
{
    private readonly FitspireDbContext _context;
    private readonly UserManager<AppUser> _userManager;
    private readonly IMapper _mapper;
    private readonly IMediaResponseFactory _mediaResponseFactory;

    public UserService(
        FitspireDbContext context,
        UserManager<AppUser> userManager,
        IMapper mapper,
        IMediaResponseFactory mediaResponseFactory)
    {
        _context = context;
        _userManager = userManager;
        _mapper = mapper;
        _mediaResponseFactory = mediaResponseFactory;
    }

    public async Task<UserProfileDto> GetProfileAsync(Guid userId)
    {
        var user = await GetUserOrThrowAsync(userId);
        return await MapProfileAsync(user);
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

        var updatedUserName = dto.UserName is null ? null : UserNameRules.Normalize(dto.UserName);
        if (dto.DisplayName != null)
            user.DisplayName = dto.DisplayName;
        if (dto.Bio != null)
            user.Bio = dto.Bio;
        if (dto.IsPrivate.HasValue)
            user.IsPrivate = dto.IsPrivate.Value;
        if (dto.FavoriteSport.HasValue)
            user.FavoriteSport = dto.FavoriteSport.Value;
        if (dto.FitnessLevel.HasValue)
            user.FitnessLevel = dto.FitnessLevel.Value;
        if (dto.HeightCm.HasValue)
            user.HeightCm = dto.HeightCm.Value;

        user.UpdatedAt = DateTime.UtcNow;
        var result = updatedUserName is null
            ? await _userManager.UpdateAsync(user)
            : await _userManager.SetUserNameAsync(user, updatedUserName);
        if (!result.Succeeded)
            throw new InvalidOperationException(string.Join("; ", result.Errors.Select(error => error.Description)));

        return await MapProfileAsync(user);
    }

    public async Task<UserProfileDto> AttachProfilePictureAsync(Guid userId, Guid mediaAssetId)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var user = await GetUserOrThrowAsync(userId);
        var asset = await GetOwnedProfileMediaOrThrowAsync(userId, mediaAssetId);

        if (user.ProfilePictureMediaId == asset.Id)
        {
            await transaction.CommitAsync();
            return await MapProfileAsync(user);
        }

        await EnsureMediaIsUnattachedAsync(asset.Id, userId);
        asset.Attach(DateTime.UtcNow);
        var previousMediaId = user.SetProfilePictureMedia(asset.Id);
        await RetirePreviousProfileMediaAsync(previousMediaId, asset.Id);

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();
        return await GetProfileAsync(userId);
    }

    public async Task<UserProfileDto> RemoveProfilePictureAsync(Guid userId)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var user = await GetUserOrThrowAsync(userId);
        var previousMediaId = user.RemoveProfilePictureMedia();

        await RetirePreviousProfileMediaAsync(previousMediaId, null);
        await _context.SaveChangesAsync();
        await transaction.CommitAsync();
        return await GetProfileAsync(userId);
    }

    public async Task<UserPreferencesDto> UpdatePreferencesAsync(Guid userId, UpdateUserPreferencesDto dto)
    {
        var user = await GetUserOrThrowAsync(userId);
        var isNewPreferences = user.AppUserPreference is null;
        var preferences = user.AppUserPreference ?? new AppUserPreference
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            CreatedAt = DateTime.UtcNow
        };

        if (dto.PreferredLanguage != null)
            preferences.PreferredLanguage = dto.PreferredLanguage;
        if (dto.IsDarkModeEnabled.HasValue)
            preferences.IsDarkModeEnabled = dto.IsDarkModeEnabled.Value;
        if (dto.ReceiveEmailNotifications.HasValue)
            preferences.ReceiveEmailNotifications = dto.ReceiveEmailNotifications.Value;
        if (dto.UnitSystem != null)
            preferences.UnitSystem = dto.UnitSystem;
        if (dto.TimeZoneId != null)
            preferences.TimeZoneId = dto.TimeZoneId;

        preferences.UpdatedAt = DateTime.UtcNow;

        // Reference-navigation fixup on SaveChanges doesn't reliably mark a brand-new
        // dependent as Added for this non-shared-key one-to-one — attach it explicitly.
        if (isNewPreferences)
        {
            user.AppUserPreference = preferences;
            _context.Add(preferences);
        }

        await _context.SaveChangesAsync();

        return _mapper.Map<UserPreferencesDto>(preferences);
    }

    private async Task<AppUser> GetUserOrThrowAsync(Guid userId)
    {
        var user = await _context.Users
            .Include(user => user.AppUserPreference)
            .Include(user => user.ProfilePictureMedia)
                .ThenInclude(media => media!.Variants)
            .FirstOrDefaultAsync(user => user.Id == userId);
        if (user is null)
            throw new UnauthorizedAccessException("User not found.");

        return user;
    }

    private async Task<MediaAsset> GetOwnedProfileMediaOrThrowAsync(Guid userId, Guid mediaAssetId)
    {
        var asset = await _context.MediaAssets
            .FirstOrDefaultAsync(asset => asset.Id == mediaAssetId && asset.OwnerUserId == userId)
            ?? throw new InvalidOperationException("Profile picture upload was not found.");

        if (asset.Purpose != MediaPurpose.ProfilePicture || asset.Status != MediaStatus.Ready)
            throw new InvalidOperationException("Profile picture media must be a ready profile-picture upload.");

        return asset;
    }

    private async Task EnsureMediaIsUnattachedAsync(Guid mediaAssetId, Guid userId)
    {
        var usedByAnotherProfile = await _context.Users.AnyAsync(user =>
            user.Id != userId && user.ProfilePictureMediaId == mediaAssetId);
        var usedByPost = await _context.PostMedia.AnyAsync(media => media.MediaAssetId == mediaAssetId);

        if (usedByAnotherProfile || usedByPost)
            throw new InvalidOperationException("Media is already attached to another resource.");
    }

    private async Task RetirePreviousProfileMediaAsync(Guid? previousMediaId, Guid? replacementMediaId)
    {
        if (!previousMediaId.HasValue || previousMediaId == replacementMediaId)
            return;

        var previous = await _context.MediaAssets.FirstOrDefaultAsync(asset => asset.Id == previousMediaId.Value);
        previous?.Retire(DateTime.UtcNow);
    }

    private async Task<UserProfileDto> MapProfileAsync(AppUser user)
    {
        var profile = _mapper.Map<UserProfileDto>(user);
        profile.ProfilePicture = await _mediaResponseFactory.CreateAsync(user.ProfilePictureMedia, CancellationToken.None);
        profile.ProfilePictureUrl = profile.ProfilePicture?.Thumbnail?.Url;
        return profile;
    }
}
