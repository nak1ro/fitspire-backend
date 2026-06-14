using AutoMapper;
using backend.Modules.Shared.Domain;
using backend.Modules.Social.Contracts.Profiles;
using backend.Modules.Social.Infrastructure;
using backend.Modules.Social.Services;
using MediatR;

namespace backend.Modules.Social.Features.Profiles;

public record GetSocialConnectionsQuery(Guid ViewerUserId, Guid TargetUserId, bool GetFollowers, int Page, int PageSize)
    : IRequest<List<SocialUserSummaryResponse>>;

public class GetSocialConnectionsHandler : IRequestHandler<GetSocialConnectionsQuery, List<SocialUserSummaryResponse>>
{
    private readonly ISocialRepository _repository;
    private readonly ISocialAccessService _accessService;
    private readonly IMapper _mapper;

    public GetSocialConnectionsHandler(ISocialRepository repository, ISocialAccessService accessService, IMapper mapper)
    {
        _repository = repository;
        _accessService = accessService;
        _mapper = mapper;
    }

    public async Task<List<SocialUserSummaryResponse>> Handle(GetSocialConnectionsQuery request, CancellationToken cancellationToken)
    {
        ValidatePagination(request.Page, request.PageSize);
        if (!await _repository.UserExistsAsync(request.TargetUserId, cancellationToken))
            throw new NotFoundException($"User {request.TargetUserId} not found.");

        if (!await _accessService.CanViewProtectedContentAsync(request.ViewerUserId, request.TargetUserId, cancellationToken))
            throw new UnauthorizedAccessException("This account's social connections are private.");

        var connections = request.GetFollowers
            ? await _repository.GetFollowersAsync(request.TargetUserId, request.Page, request.PageSize, cancellationToken)
            : await _repository.GetFollowingAsync(request.TargetUserId, request.Page, request.PageSize, cancellationToken);

        var users = request.GetFollowers
            ? connections.Select(connection => connection.FollowerUser)
            : connections.Select(connection => connection.FollowedUser);
        return _mapper.Map<List<SocialUserSummaryResponse>>(users);
    }

    private static void ValidatePagination(int page, int pageSize)
    {
        if (page < 1 || pageSize is < 1 or > 100)
            throw new DomainException("Page must be at least one and page size must be between 1 and 100.");
    }
}
