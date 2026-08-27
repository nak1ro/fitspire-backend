using backend.Modules.Media.Contracts;
using backend.Modules.Social.Contracts.Profiles;
using backend.Modules.Social.Features.Common;
using backend.Modules.Social.Infrastructure;
using FluentValidation;
using MediatR;

namespace backend.Modules.Social.Features.Profiles;

public record DiscoverSocialUsersQuery(Guid ViewerUserId, string? Query, int Limit)
    : IRequest<List<DiscoverableSocialUserResponse>>;

public class DiscoverSocialUsersValidator : AbstractValidator<DiscoverSocialUsersQuery>
{
    public DiscoverSocialUsersValidator()
    {
        RuleFor(query => query.Limit).InclusiveBetween(1, 10);
        RuleFor(query => query.Query)
            .MinimumLength(2)
            .MaximumLength(80)
            .When(query => !string.IsNullOrWhiteSpace(query.Query));
    }
}

public class DiscoverSocialUsersHandler
    : IRequestHandler<DiscoverSocialUsersQuery, List<DiscoverableSocialUserResponse>>
{
    private readonly ISocialRepository _socialRepository;
    private readonly IMediaResponseFactory _mediaResponseFactory;

    public DiscoverSocialUsersHandler(
        ISocialRepository socialRepository,
        IMediaResponseFactory mediaResponseFactory)
    {
        _socialRepository = socialRepository;
        _mediaResponseFactory = mediaResponseFactory;
    }

    public async Task<List<DiscoverableSocialUserResponse>> Handle(
        DiscoverSocialUsersQuery request,
        CancellationToken cancellationToken)
    {
        var query = request.Query?.Trim();
        var users = string.IsNullOrWhiteSpace(query)
            ? await _socialRepository.GetRecommendedUsersAsync(request.ViewerUserId, request.Limit, cancellationToken)
            : await _socialRepository.SearchDiscoverableUsersAsync(request.ViewerUserId, query, request.Limit, cancellationToken);

        var pictures = await SocialUserResponseMapper.GetProfilePicturesAsync(users, _mediaResponseFactory, cancellationToken);
        return users.Select(user => MapUser(user, pictures, query)).ToList();
    }

    private static DiscoverableSocialUserResponse MapUser(
        backend.Modules.User.Domain.AppUser user,
        IReadOnlyDictionary<Guid, backend.Modules.Media.Contracts.MediaResponse> pictures,
        string? query)
    {
        var profilePicture = user.ProfilePictureMedia is null ? null : pictures.GetValueOrDefault(user.ProfilePictureMedia.Id);
        var reason = query is null ? "Active in Fitspire" : null;
        return new DiscoverableSocialUserResponse(
            user.Id, user.UserName ?? string.Empty, user.DisplayName,
            profilePicture?.Thumbnail?.Url, profilePicture, reason);
    }
}
