using backend.Modules.Shared;
using backend.Modules.Shared.Domain;
using backend.Modules.Social.Domain;
using backend.Modules.Social.Domain.Enums;
using backend.Modules.Social.Infrastructure;
using backend.Modules.Workout.Domain.Enums;
using backend.Modules.Workout.Infrastructure;
using MediatR;

namespace backend.Modules.Social.Features.Posts;

public record ShareWorkoutCommand(Guid UserId, Guid WorkoutId, string? Caption = null) : IRequest<Guid>;

public class ShareWorkoutHandler : IRequestHandler<ShareWorkoutCommand, Guid>
{
    private readonly IWorkoutRepository _workoutRepository;
    private readonly ISocialRepository _socialRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ShareWorkoutHandler(
        IWorkoutRepository workoutRepository,
        ISocialRepository socialRepository,
        IUnitOfWork unitOfWork)
    {
        _workoutRepository = workoutRepository;
        _socialRepository = socialRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(ShareWorkoutCommand request, CancellationToken cancellationToken)
    {
        var workout = (await _workoutRepository.GetByIdsAsync([request.WorkoutId], cancellationToken)).SingleOrDefault();
        if (workout is null || workout.UserId != request.UserId)
            throw new NotFoundException($"Workout {request.WorkoutId} not found.");

        if (workout.Status != WorkoutStatus.Completed)
            throw new DomainException("Only completed workouts can be shared.");

        var existingPost = await _socialRepository.GetPostByReferenceAsync(
            PostType.WorkoutShare,
            request.WorkoutId,
            cancellationToken);
        if (existingPost is not null)
            return existingPost.Id;

        var snapshot = new WorkoutShareSnapshot(
            workout.Id,
            workout.WorkoutType,
            workout.Date,
            workout.DurationMinutes,
            workout.GetTotalDistance(),
            workout.CaloriesBurned,
            workout.GetTotalVolume(),
            workout.GetExerciseCount(),
            workout.CompletedAt);
        var post = Post.CreateWorkoutSharePost(request.UserId, snapshot, request.Caption);

        await _socialRepository.AddPostAsync(post, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return post.Id;
    }
}
