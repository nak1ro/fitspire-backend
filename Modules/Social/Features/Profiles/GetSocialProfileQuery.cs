using backend.Modules.Shared.Domain;
using backend.Modules.Media.Contracts;
using backend.Modules.Social.Contracts.Profiles;
using backend.Modules.Social.Features.Common;
using backend.Modules.Social.Infrastructure;
using MediatR;

namespace backend.Modules.Social.Features.Profiles;

public record GetSocialProfileQuery(Guid ViewerUserId, Guid TargetUserId) : IRequest<SocialProfileResponse>;

public class GetSocialProfileHandler : IRequestHandler<GetSocialProfileQuery, SocialProfileResponse>
{
    private readonly ISocialRepository _socialRepository;
    private readonly IMediaResponseFactory _mediaResponseFactory;

    public GetSocialProfileHandler(ISocialRepository socialRepository, IMediaResponseFactory mediaResponseFactory)
    {
        _socialRepository = socialRepository;
        _mediaResponseFactory = mediaResponseFactory;
    }

    public async Task<SocialProfileResponse> Handle(GetSocialProfileQuery request, CancellationToken cancellationToken)
    {
        var user = await _socialRepository.GetSocialUserAsync(request.TargetUserId, cancellationToken);
        if (user is null)
            throw new NotFoundException($"User {request.TargetUserId} not found.");

        var relationship = await GetRelationshipAsync(request.ViewerUserId, user.Id, cancellationToken);
        var followersCount = await _socialRepository.GetFollowersCountAsync(user.Id, cancellationToken);
        var followingCount = await _socialRepository.GetFollowingCountAsync(user.Id, cancellationToken);
        var pictures = await SocialUserResponseMapper.GetProfilePicturesAsync([user], _mediaResponseFactory, cancellationToken);
        var profilePicture = user.ProfilePictureMedia is null ? null : pictures.GetValueOrDefault(user.ProfilePictureMedia.Id);

        return new SocialProfileResponse(
            user.Id,
            user.UserName ?? string.Empty,
            user.DisplayName,
            user.Bio,
            profilePicture?.Thumbnail?.Url,
            profilePicture,
            user.IsPrivate,
            followersCount,
            followingCount,
            relationship);
    }

    private async Task<string> GetRelationshipAsync(Guid viewerUserId, Guid targetUserId, CancellationToken cancellationToken)
    {
        if (viewerUserId == targetUserId)
            return "self";

        if (await _socialRepository.IsFollowingAsync(viewerUserId, targetUserId, cancellationToken))
            return "following";

        return await _socialRepository.HasPendingFollowRequestAsync(viewerUserId, targetUserId, cancellationToken)
            ? "outgoing-request-pending"
            : "not-following";
    }
}
