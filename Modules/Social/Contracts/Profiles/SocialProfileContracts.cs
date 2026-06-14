namespace backend.Modules.Social.Contracts.Profiles;

public record SocialUserSummaryResponse(
    Guid Id,
    string UserName,
    string DisplayName,
    string? ProfilePictureUrl);

public record SocialProfileResponse(
    Guid Id,
    string UserName,
    string DisplayName,
    string? Bio,
    string? ProfilePictureUrl,
    bool IsPrivate,
    int FollowersCount,
    int FollowingCount,
    string Relationship);
