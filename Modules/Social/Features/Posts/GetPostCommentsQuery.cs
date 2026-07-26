using backend.Modules.Shared.Domain;
using backend.Modules.Media.Contracts;
using backend.Modules.Social.Contracts.Comments;
using backend.Modules.Social.Features.Common;
using backend.Modules.Social.Infrastructure;
using backend.Modules.Social.Services;
using backend.Modules.User.Domain;
using MediatR;

namespace backend.Modules.Social.Features.Posts;

public record GetPostCommentsQuery(Guid ViewerUserId, Guid PostId, int Page, int PageSize) : IRequest<List<CommentResponse>>;

public class GetPostCommentsHandler : IRequestHandler<GetPostCommentsQuery, List<CommentResponse>>
{
    private readonly ISocialRepository _repository;
    private readonly ISocialAccessService _accessService;
    private readonly IMediaResponseFactory _mediaResponseFactory;

    public GetPostCommentsHandler(ISocialRepository repository, ISocialAccessService accessService, IMediaResponseFactory mediaResponseFactory)
    {
        _repository = repository;
        _accessService = accessService;
        _mediaResponseFactory = mediaResponseFactory;
    }

    public async Task<List<CommentResponse>> Handle(GetPostCommentsQuery request, CancellationToken cancellationToken)
    {
        var post = await _repository.GetPostByIdAsync(request.PostId, cancellationToken)
            ?? throw new NotFoundException($"Post {request.PostId} not found.");
        if (!await _accessService.CanViewProtectedContentAsync(request.ViewerUserId, post.UserId, cancellationToken))
            throw new NotFoundException($"Post {request.PostId} not found.");

        ValidatePagination(request.Page, request.PageSize);
        var comments = await _repository.GetTopLevelCommentsAsync(request.PostId, request.Page, request.PageSize, cancellationToken);
        var replyCounts = await _repository.GetReplyCountsAsync(
            request.PostId,
            comments.Select(comment => comment.Id),
            cancellationToken);
        var avatars = await SocialUserResponseMapper.GetProfilePicturesAsync(
            comments.Select(comment => comment.User)
                .Concat(comments.Select(comment => comment.ReplyToComment?.User).OfType<AppUser>()),
            _mediaResponseFactory, cancellationToken);
        return comments.Select(comment => new CommentResponse(
            comment.Id, comment.UserId, comment.User.UserName ?? "Unknown",
            GetAvatarUrl(comment.User, avatars), GetAvatar(comment.User, avatars),
            comment.Content, comment.RootCommentId, comment.ReplyToCommentId, comment.Likes.Count,
            comment.Likes.Any(like => like.UserId == request.ViewerUserId),
            replyCounts.GetValueOrDefault(comment.Id),
            comment.CreatedAt, comment.UpdatedAt,
            comment.ReplyToComment is null ? null : SocialUserResponseMapper.MapSummary(comment.ReplyToComment.User, avatars))).ToList();
    }

    private static MediaResponse? GetAvatar(AppUser user, IReadOnlyDictionary<Guid, MediaResponse> avatars) =>
        user.ProfilePictureMedia is null ? null : avatars.GetValueOrDefault(user.ProfilePictureMedia.Id);

    private static string? GetAvatarUrl(AppUser user, IReadOnlyDictionary<Guid, MediaResponse> avatars) =>
        GetAvatar(user, avatars)?.Thumbnail?.Url;

    private static void ValidatePagination(int page, int pageSize)
    {
        if (page < 1 || pageSize is < 1 or > 100)
            throw new DomainException("Page must be at least one and page size must be between 1 and 100.");
    }
}
