using backend.Modules.Shared;
using backend.Modules.Shared.Domain;
using backend.Modules.Social.Infrastructure;
using MediatR;

namespace backend.Modules.Social.Features.Posts;

public record DeleteCommentCommand(Guid UserId, Guid PostId, Guid CommentId) : IRequest;

public class DeleteCommentHandler : IRequestHandler<DeleteCommentCommand>
{
    private readonly ISocialRepository _socialRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteCommentHandler(ISocialRepository socialRepository, IUnitOfWork unitOfWork)
    {
        _socialRepository = socialRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeleteCommentCommand request, CancellationToken cancellationToken)
    {
        var comment = await _socialRepository.GetCommentByIdAsync(
            request.PostId,
            request.CommentId,
            cancellationToken);

        if (comment == null)
        {
            throw new NotFoundException($"Comment {request.CommentId} not found.");
        }

        if (!comment.CanBeDeletedBy(request.UserId))
        {
            throw new UnauthorizedAccessException("Comment cannot be deleted by the current user.");
        }

        await _socialRepository.DeleteCommentAsync(comment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
