using backend.Modules.Media.Contracts;
using backend.Modules.Shared.Domain;
using backend.Modules.Social.Contracts.Profiles;
using backend.Modules.Social.Features.Common;
using backend.Modules.Social.Infrastructure;
using backend.Modules.Social.Services;
using MediatR;

namespace backend.Modules.Social.Features.Posts;

public record GetPostLikedByQuery(Guid ViewerUserId, Guid PostId, int Page, int PageSize)
    : IRequest<List<SocialUserSummaryResponse>>;

public record GetCommentLikedByQuery(Guid ViewerUserId, Guid PostId, Guid CommentId, int Page, int PageSize)
    : IRequest<List<SocialUserSummaryResponse>>;

public class GetPostLikedByHandler : IRequestHandler<GetPostLikedByQuery, List<SocialUserSummaryResponse>>
{
    private readonly ISocialRepository _repository;
    private readonly ISocialAccessService _accessService;
    private readonly IMediaResponseFactory _mediaResponseFactory;

    public GetPostLikedByHandler(ISocialRepository repository, ISocialAccessService accessService, IMediaResponseFactory mediaResponseFactory)
    {
        _repository = repository;
        _accessService = accessService;
        _mediaResponseFactory = mediaResponseFactory;
    }

    public async Task<List<SocialUserSummaryResponse>> Handle(GetPostLikedByQuery request, CancellationToken cancellationToken)
    {
        ValidatePagination(request.Page, request.PageSize);
        var post = await _repository.GetPostByIdAsync(request.PostId, cancellationToken)
            ?? throw new NotFoundException($"Post {request.PostId} not found.");

        if (!await _accessService.CanViewProtectedContentAsync(request.ViewerUserId, post.UserId, cancellationToken))
            throw new NotFoundException($"Post {request.PostId} not found.");

        var users = await _repository.GetPostLikersAsync(request.PostId, request.Page, request.PageSize, cancellationToken);
        var pictures = await SocialUserResponseMapper.GetProfilePicturesAsync(users, _mediaResponseFactory, cancellationToken);
        return users.Select(user => SocialUserResponseMapper.MapSummary(user, pictures)).ToList();
    }

    private static void ValidatePagination(int page, int pageSize)
    {
        if (page < 1 || pageSize is < 1 or > 100)
            throw new DomainException("Page must be at least one and page size must be between 1 and 100.");
    }
}

public class GetCommentLikedByHandler : IRequestHandler<GetCommentLikedByQuery, List<SocialUserSummaryResponse>>
{
    private readonly ISocialRepository _repository;
    private readonly ISocialAccessService _accessService;
    private readonly IMediaResponseFactory _mediaResponseFactory;

    public GetCommentLikedByHandler(ISocialRepository repository, ISocialAccessService accessService, IMediaResponseFactory mediaResponseFactory)
    {
        _repository = repository;
        _accessService = accessService;
        _mediaResponseFactory = mediaResponseFactory;
    }

    public async Task<List<SocialUserSummaryResponse>> Handle(GetCommentLikedByQuery request, CancellationToken cancellationToken)
    {
        ValidatePagination(request.Page, request.PageSize);
        var comment = await _repository.GetCommentByIdAsync(request.PostId, request.CommentId, cancellationToken)
            ?? throw new NotFoundException($"Comment {request.CommentId} not found.");

        if (!await _accessService.CanViewProtectedContentAsync(request.ViewerUserId, comment.Post.UserId, cancellationToken))
            throw new NotFoundException($"Comment {request.CommentId} not found.");

        var users = await _repository.GetCommentLikersAsync(request.CommentId, request.Page, request.PageSize, cancellationToken);
        var pictures = await SocialUserResponseMapper.GetProfilePicturesAsync(users, _mediaResponseFactory, cancellationToken);
        return users.Select(user => SocialUserResponseMapper.MapSummary(user, pictures)).ToList();
    }

    private static void ValidatePagination(int page, int pageSize)
    {
        if (page < 1 || pageSize is < 1 or > 100)
            throw new DomainException("Page must be at least one and page size must be between 1 and 100.");
    }
}
