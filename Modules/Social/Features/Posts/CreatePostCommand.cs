using backend.Modules.Social.Domain;
using backend.Modules.Social.Infrastructure;
using backend.Modules.Shared;
using MediatR;

namespace backend.Modules.Social.Features.Posts;

public record CreatePostCommand(Guid UserId, string Content, string? ImageUrl = null) : IRequest<Guid>;

public class CreatePostHandler : IRequestHandler<CreatePostCommand, Guid>
{
    private readonly ISocialRepository _socialRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreatePostHandler(ISocialRepository socialRepository, IUnitOfWork unitOfWork)
    {
        _socialRepository = socialRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreatePostCommand request, CancellationToken cancellationToken)
    {
        var post = Post.CreateTextPost(request.UserId, request.Content, request.ImageUrl);

        await _socialRepository.AddPostAsync(post, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return post.Id;
    }
}
