using backend.Modules.Shared.Domain;
using backend.Modules.Media.Contracts;
using backend.Modules.Social.Contracts.Comments;
using backend.Modules.Social.Features.Common;
using backend.Modules.Social.Infrastructure;
using backend.Modules.Social.Services;
using backend.Modules.User.Domain;
using MediatR;

namespace backend.Modules.Social.Features.Posts;

public record GetCommentRepliesQuery(Guid ViewerUserId, Guid PostId, Guid CommentId, int Page, int PageSize)
    : IRequest<List<CommentResponse>>;

public class GetCommentRepliesHandler : IRequestHandler<GetCommentRepliesQuery, List<CommentResponse>>
{
    private readonly ISocialRepository _repository;
    private readonly ISocialAccessService _accessService;
    private readonly IMediaResponseFactory _mediaResponseFactory;

    public GetCommentRepliesHandler(ISocialRepository repository, ISocialAccessService accessService, IMediaResponseFactory mediaResponseFactory)
    {
        _repository = repository;
        _accessService = accessService;
        _mediaResponseFactory = mediaResponseFactory;
    }

    public async Task<List<CommentResponse>> Handle(GetCommentRepliesQuery request, CancellationToken cancellationToken)
    {
        ValidatePagination(request.Page, request.PageSize);
        var comment = await _repository.GetCommentByIdAsync(request.PostId, request.CommentId, cancellationToken)
            ?? throw new NotFoundException($"Comment {request.CommentId} not found.");

        if (!await _accessService.CanViewProtectedContentAsync(request.ViewerUserId, comment.Post.UserId, cancellationToken))
            throw new NotFoundException($"Comment {request.CommentId} not found.");

        var rootCommentId = comment.RootCommentId ?? comment.Id;
        var replies = await _repository.GetCommentRepliesAsync(request.PostId, rootCommentId, request.Page, request.PageSize, cancellationToken);
        var avatars = await SocialUserResponseMapper.GetProfilePicturesAsync(
            replies.Select(reply => reply.User), _mediaResponseFactory, cancellationToken);
        return replies.Select(reply => MapResponse(reply, request.ViewerUserId, avatars)).ToList();
    }

    private static CommentResponse MapResponse(Domain.Comment comment, Guid viewerUserId,
        IReadOnlyDictionary<Guid, MediaResponse> avatars) => new(
        comment.Id, comment.UserId, comment.User.UserName ?? "Unknown", GetAvatarUrl(comment.User, avatars),
        GetAvatar(comment.User, avatars),
        comment.Content, comment.RootCommentId, comment.ReplyToCommentId, comment.Likes.Count,
        comment.Likes.Any(like => like.UserId == viewerUserId), 0,
        comment.CreatedAt, comment.UpdatedAt);

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
