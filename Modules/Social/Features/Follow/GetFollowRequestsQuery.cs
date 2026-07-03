using backend.Modules.Shared.Domain;
using backend.Modules.Media.Contracts;
using backend.Modules.Social.Contracts.Follows;
using backend.Modules.Social.Domain;
using backend.Modules.Social.Features.Common;
using backend.Modules.Social.Infrastructure;
using MediatR;

namespace backend.Modules.Social.Features.Follow;

public record GetFollowRequestsQuery(Guid UserId, bool IsIncoming, int Page, int PageSize) : IRequest<List<FollowRequestResponse>>;

public class GetFollowRequestsHandler : IRequestHandler<GetFollowRequestsQuery, List<FollowRequestResponse>>
{
    private readonly ISocialRepository _repository;
    private readonly IMediaResponseFactory _mediaResponseFactory;

    public GetFollowRequestsHandler(ISocialRepository repository, IMediaResponseFactory mediaResponseFactory)
    {
        _repository = repository;
        _mediaResponseFactory = mediaResponseFactory;
    }

    public async Task<List<FollowRequestResponse>> Handle(GetFollowRequestsQuery request, CancellationToken cancellationToken)
    {
        ValidatePagination(request.Page, request.PageSize);
        var requests = request.IsIncoming
            ? await _repository.GetIncomingFollowRequestsAsync(request.UserId, request.Page, request.PageSize, cancellationToken)
            : await _repository.GetOutgoingFollowRequestsAsync(request.UserId, request.Page, request.PageSize, cancellationToken);

        var users = requests.Select(followRequest => request.IsIncoming ? followRequest.Requester : followRequest.Addressee);
        var pictures = await SocialUserResponseMapper.GetProfilePicturesAsync(users, _mediaResponseFactory, cancellationToken);
        return requests.Select(followRequest => Map(followRequest, request.IsIncoming, pictures)).ToList();
    }

    private static FollowRequestResponse Map(FollowRequest followRequest, bool isIncoming,
        IReadOnlyDictionary<Guid, MediaResponse> pictures)
    {
        var user = isIncoming ? followRequest.Requester : followRequest.Addressee;
        return new FollowRequestResponse(
            followRequest.Id,
            user.Id,
            user.UserName ?? string.Empty,
            user.DisplayName,
            GetProfilePicture(user, pictures)?.Thumbnail?.Url,
            GetProfilePicture(user, pictures),
            followRequest.RequestedAt);
    }

    private static MediaResponse? GetProfilePicture(
        backend.Modules.User.Domain.AppUser user,
        IReadOnlyDictionary<Guid, MediaResponse> pictures) =>
        user.ProfilePictureMedia is null ? null : pictures.GetValueOrDefault(user.ProfilePictureMedia.Id);

    private static void ValidatePagination(int page, int pageSize)
    {
        if (page < 1 || pageSize is < 1 or > 100)
            throw new DomainException("Page must be at least one and page size must be between 1 and 100.");
    }
}
