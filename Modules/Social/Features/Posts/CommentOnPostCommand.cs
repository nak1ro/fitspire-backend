using backend.Modules.Social.Domain;
using backend.Modules.Social.Infrastructure;
using backend.Modules.Shared;
using MediatR;

namespace backend.Modules.Social.Features.Posts;

public record CommentOnPostCommand(Guid UserId, Guid PostId, string Content) : IRequest<Guid>;

public class CommentOnPostHandler : IRequestHandler<CommentOnPostCommand, Guid>
{
    private readonly ISocialRepository _socialRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CommentOnPostHandler(ISocialRepository socialRepository, IUnitOfWork unitOfWork)
    {
        _socialRepository = socialRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CommentOnPostCommand request, CancellationToken cancellationToken)
    {
        var comment = new Comment(request.PostId, request.UserId, request.Content);

        await _socialRepository.AddCommentAsync(comment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return comment.Id;
    }
}
