using backend.Modules.Shared;
using backend.Modules.Shared.Domain;
using backend.Modules.Social.Infrastructure;
using MediatR;

namespace backend.Modules.Social.Features.Posts;

public record DeletePostCommand(Guid UserId, Guid PostId) : IRequest;

public class DeletePostHandler : IRequestHandler<DeletePostCommand>
{
    private readonly ISocialRepository _socialRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeletePostHandler(ISocialRepository socialRepository, IUnitOfWork unitOfWork)
    {
        _socialRepository = socialRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeletePostCommand request, CancellationToken cancellationToken)
    {
        var post = await _socialRepository.GetPostByIdAsync(request.PostId, cancellationToken);

        if (post == null)
        {
            throw new NotFoundException($"Post {request.PostId} not found.");
        }

        if (post.UserId != request.UserId)
        {
            throw new UnauthorizedAccessException("Post does not belong to the current user.");
        }

        await _socialRepository.DeletePostAsync(post, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
