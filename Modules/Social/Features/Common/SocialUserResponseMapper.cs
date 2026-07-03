using backend.Modules.Media.Contracts;
using backend.Modules.Social.Contracts.Profiles;
using backend.Modules.User.Domain;

namespace backend.Modules.Social.Features.Common;

public static class SocialUserResponseMapper
{
    public static async Task<IReadOnlyDictionary<Guid, MediaResponse>> GetProfilePicturesAsync(
        IEnumerable<AppUser> users,
        IMediaResponseFactory mediaResponseFactory,
        CancellationToken cancellationToken)
    {
        return await mediaResponseFactory.CreateManyAsync(
            users.Select(user => user.ProfilePictureMedia).OfType<backend.Modules.Media.Domain.MediaAsset>(),
            cancellationToken);
    }

    public static SocialUserSummaryResponse MapSummary(
        AppUser user,
        IReadOnlyDictionary<Guid, MediaResponse> profilePictures)
    {
        var profilePicture = user.ProfilePictureMedia is null
            ? null
            : profilePictures.GetValueOrDefault(user.ProfilePictureMedia.Id);
        return new SocialUserSummaryResponse(user.Id, user.UserName ?? string.Empty, user.DisplayName,
            profilePicture?.Thumbnail?.Url, profilePicture);
    }
}
