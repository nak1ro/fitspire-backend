using backend.Modules.Media.Contracts;
using backend.Modules.User.Domain.Enums;

namespace backend.Modules.Social.Contracts.Profiles;

public record SocialUserSummaryResponse(
    Guid Id,
    string UserName,
    string DisplayName,
    string? ProfilePictureUrl,
    MediaResponse? ProfilePicture);

public record DiscoverableSocialUserResponse(
    Guid Id,
    string UserName,
    string DisplayName,
    string? ProfilePictureUrl,
    MediaResponse? ProfilePicture,
    string? Reason);

public record SocialProfileResponse(
    Guid Id,
    string UserName,
    string DisplayName,
    string? Bio,
    string? ProfilePictureUrl,
    MediaResponse? ProfilePicture,
    bool IsPrivate,
    int FollowersCount,
    int FollowingCount,
    string Relationship,
    FitnessSport? FavoriteSport,
    FitnessLevel? FitnessLevel);
