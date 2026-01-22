using backend.Modules.Social.Domain;
using backend.Modules.Social.Infrastructure;
using backend.Modules.Shared;
using MediatR;

namespace backend.Modules.Social.Features.Feed;

public record GetUserFeedQuery(Guid UserId, int Page = 1, int PageSize = 20) : IRequest<List<FeedItemResponse>>;

public record FeedItemResponse(
    Guid Id,
    Guid UserId,
    string UserName,
    string? UserAvatarUrl,
    string Type,
    string? Content,
    Guid? ReferenceEntityId,
    int LikesCount,
    int CommentsCount,
    DateTime CreatedAt
);

public class GetUserFeedHandler : IRequestHandler<GetUserFeedQuery, List<FeedItemResponse>>
{
    private readonly ISocialRepository _socialRepository;

    public GetUserFeedHandler(ISocialRepository socialRepository)
    {
        _socialRepository = socialRepository;
    }

    public async Task<List<FeedItemResponse>> Handle(GetUserFeedQuery request, CancellationToken cancellationToken)
    {
        var posts = await _socialRepository.GetUserFeedAsync(request.UserId, request.Page, request.PageSize, cancellationToken);

        return posts.Select(p => new FeedItemResponse(
            p.Id,
            p.UserId,
            p.User?.UserName ?? "Unknown",
            p.User?.ProfilePictureUrl,
            p.Type.ToString(),
            p.Content,
            p.ReferenceEntityId,
            p.Likes.Count,
            p.Comments.Count,
            p.CreatedAt
        )).ToList();
    }
}
