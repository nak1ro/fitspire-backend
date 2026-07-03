using backend.Modules.Media.Contracts;

namespace backend.Modules.Social.Contracts.Follows;

public record FollowRequestResponse(
    Guid Id,
    Guid UserId,
    string UserName,
    string DisplayName,
    string? ProfilePictureUrl,
    MediaResponse? ProfilePicture,
    DateTime RequestedAt);
