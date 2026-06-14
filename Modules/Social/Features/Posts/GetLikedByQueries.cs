using AutoMapper;
using backend.Modules.Shared.Domain;
using backend.Modules.Social.Contracts.Profiles;
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
    private readonly IMapper _mapper;

    public GetPostLikedByHandler(ISocialRepository repository, ISocialAccessService accessService, IMapper mapper)
    {
        _repository = repository;
        _accessService = accessService;
        _mapper = mapper;
    }

    public async Task<List<SocialUserSummaryResponse>> Handle(GetPostLikedByQuery request, CancellationToken cancellationToken)
    {
        ValidatePagination(request.Page, request.PageSize);
        var post = await _repository.GetPostByIdAsync(request.PostId, cancellationToken)
            ?? throw new NotFoundException($"Post {request.PostId} not found.");

        if (!await _accessService.CanViewProtectedContentAsync(request.ViewerUserId, post.UserId, cancellationToken))
            throw new NotFoundException($"Post {request.PostId} not found.");

        var users = await _repository.GetPostLikersAsync(request.PostId, request.Page, request.PageSize, cancellationToken);
        return _mapper.Map<List<SocialUserSummaryResponse>>(users);
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
    private readonly IMapper _mapper;

    public GetCommentLikedByHandler(ISocialRepository repository, ISocialAccessService accessService, IMapper mapper)
    {
        _repository = repository;
        _accessService = accessService;
        _mapper = mapper;
    }

    public async Task<List<SocialUserSummaryResponse>> Handle(GetCommentLikedByQuery request, CancellationToken cancellationToken)
    {
        ValidatePagination(request.Page, request.PageSize);
        var comment = await _repository.GetCommentByIdAsync(request.PostId, request.CommentId, cancellationToken)
            ?? throw new NotFoundException($"Comment {request.CommentId} not found.");

        if (!await _accessService.CanViewProtectedContentAsync(request.ViewerUserId, comment.Post.UserId, cancellationToken))
            throw new NotFoundException($"Comment {request.CommentId} not found.");

        var users = await _repository.GetCommentLikersAsync(request.CommentId, request.Page, request.PageSize, cancellationToken);
        return _mapper.Map<List<SocialUserSummaryResponse>>(users);
    }

    private static void ValidatePagination(int page, int pageSize)
    {
        if (page < 1 || pageSize is < 1 or > 100)
            throw new DomainException("Page must be at least one and page size must be between 1 and 100.");
    }
}
