using backend.Modules.Shared.Domain;
using backend.Modules.Social.Contracts.Comments;
using backend.Modules.Social.Infrastructure;
using backend.Modules.Social.Services;
using MediatR;

namespace backend.Modules.Social.Features.Posts;

public record GetPostCommentsQuery(Guid ViewerUserId, Guid PostId, int Page, int PageSize) : IRequest<List<CommentResponse>>;

public class GetPostCommentsHandler : IRequestHandler<GetPostCommentsQuery, List<CommentResponse>>
{
    private readonly ISocialRepository _repository;
    private readonly ISocialAccessService _accessService;

    public GetPostCommentsHandler(ISocialRepository repository, ISocialAccessService accessService)
    {
        _repository = repository;
        _accessService = accessService;
    }

    public async Task<List<CommentResponse>> Handle(GetPostCommentsQuery request, CancellationToken cancellationToken)
    {
        var post = await _repository.GetPostByIdAsync(request.PostId, cancellationToken)
            ?? throw new NotFoundException($"Post {request.PostId} not found.");
        if (!await _accessService.CanViewProtectedContentAsync(request.ViewerUserId, post.UserId, cancellationToken))
            throw new NotFoundException($"Post {request.PostId} not found.");

        ValidatePagination(request.Page, request.PageSize);
        var comments = await _repository.GetTopLevelCommentsAsync(request.PostId, request.Page, request.PageSize, cancellationToken);
        return comments.Select(comment => new CommentResponse(
            comment.Id, comment.UserId, comment.User.UserName ?? "Unknown", comment.User.ProfilePictureUrl,
            comment.Content, comment.RootCommentId, comment.ReplyToCommentId, comment.Likes.Count,
            comment.CreatedAt, comment.UpdatedAt)).ToList();
    }

    private static void ValidatePagination(int page, int pageSize)
    {
        if (page < 1 || pageSize is < 1 or > 100)
            throw new DomainException("Page must be at least one and page size must be between 1 and 100.");
    }
}
