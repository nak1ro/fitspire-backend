namespace backend.Modules.Social.Contracts.Follows;

public record FollowRequestResponse(
    Guid Id,
    Guid UserId,
    string UserName,
    string DisplayName,
    string? ProfilePictureUrl,
    DateTime RequestedAt);
