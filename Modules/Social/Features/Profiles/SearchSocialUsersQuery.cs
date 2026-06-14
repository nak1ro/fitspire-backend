using AutoMapper;
using backend.Modules.Shared.Domain;
using backend.Modules.Social.Contracts.Profiles;
using backend.Modules.Social.Infrastructure;
using MediatR;

namespace backend.Modules.Social.Features.Profiles;

public record SearchSocialUsersQuery(string Query, int Page, int PageSize) : IRequest<List<SocialUserSummaryResponse>>;

public class SearchSocialUsersHandler : IRequestHandler<SearchSocialUsersQuery, List<SocialUserSummaryResponse>>
{
    private readonly ISocialRepository _socialRepository;
    private readonly IMapper _mapper;

    public SearchSocialUsersHandler(ISocialRepository socialRepository, IMapper mapper)
    {
        _socialRepository = socialRepository;
        _mapper = mapper;
    }

    public async Task<List<SocialUserSummaryResponse>> Handle(SearchSocialUsersQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Query) || request.Query.Trim().Length < 2)
            throw new DomainException("Search query must contain at least two characters.");

        var users = await _socialRepository.SearchSocialUsersAsync(
            request.Query.Trim(), request.Page, request.PageSize, cancellationToken);

        return _mapper.Map<List<SocialUserSummaryResponse>>(users);
    }
}
