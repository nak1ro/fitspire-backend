using backend.Modules.Shared;
using backend.Modules.Shared.Domain;
using backend.Modules.Social.Infrastructure;
using MediatR;

namespace backend.Modules.Social.Features.Posts;

public record UpdateCommentCommand(Guid UserId, Guid PostId, Guid CommentId, string Content) : IRequest;

public class UpdateCommentHandler : IRequestHandler<UpdateCommentCommand>
{
    private readonly ISocialRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateCommentHandler(ISocialRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(UpdateCommentCommand request, CancellationToken cancellationToken)
    {
        var comment = await _repository.GetCommentByIdAsync(request.PostId, request.CommentId, cancellationToken)
            ?? throw new NotFoundException($"Comment {request.CommentId} not found.");
        if (comment.UserId != request.UserId)
            throw new UnauthorizedAccessException("Only the comment author can edit this comment.");

        comment.UpdateContent(request.Content);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
