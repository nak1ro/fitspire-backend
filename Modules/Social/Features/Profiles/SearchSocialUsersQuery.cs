using backend.Modules.Media.Contracts;
using backend.Modules.Shared.Domain;
using backend.Modules.Social.Contracts.Profiles;
using backend.Modules.Social.Features.Common;
using backend.Modules.Social.Infrastructure;
using MediatR;

namespace backend.Modules.Social.Features.Profiles;

public record SearchSocialUsersQuery(string Query, int Page, int PageSize) : IRequest<List<SocialUserSummaryResponse>>;

public class SearchSocialUsersHandler : IRequestHandler<SearchSocialUsersQuery, List<SocialUserSummaryResponse>>
{
    private readonly ISocialRepository _socialRepository;
    private readonly IMediaResponseFactory _mediaResponseFactory;

    public SearchSocialUsersHandler(ISocialRepository socialRepository, IMediaResponseFactory mediaResponseFactory)
    {
        _socialRepository = socialRepository;
        _mediaResponseFactory = mediaResponseFactory;
    }

    public async Task<List<SocialUserSummaryResponse>> Handle(SearchSocialUsersQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Query) || request.Query.Trim().Length < 2)
            throw new DomainException("Search query must contain at least two characters.");

        var users = await _socialRepository.SearchSocialUsersAsync(
            request.Query.Trim(), request.Page, request.PageSize, cancellationToken);

        var pictures = await SocialUserResponseMapper.GetProfilePicturesAsync(users, _mediaResponseFactory, cancellationToken);
        return users.Select(user => SocialUserResponseMapper.MapSummary(user, pictures)).ToList();
    }
}
