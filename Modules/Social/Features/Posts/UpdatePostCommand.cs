using backend.Modules.Shared;
using backend.Modules.Shared.Domain;
using backend.Modules.Social.Infrastructure;
using MediatR;

namespace backend.Modules.Social.Features.Posts;

public record UpdatePostCommand(Guid UserId, Guid PostId, string Content, string? ImageUrl = null) : IRequest;

public class UpdatePostHandler : IRequestHandler<UpdatePostCommand>
{
    private readonly ISocialRepository _socialRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdatePostHandler(ISocialRepository socialRepository, IUnitOfWork unitOfWork)
    {
        _socialRepository = socialRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(UpdatePostCommand request, CancellationToken cancellationToken)
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

        post.UpdateTextPost(request.Content, request.ImageUrl);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
