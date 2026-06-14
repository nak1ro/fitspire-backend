using backend.Modules.Shared;
using backend.Modules.Social.Domain;
using backend.Modules.Social.Domain.Enums;
using backend.Modules.Social.Infrastructure;
using backend.Modules.Workout.Domain.Events;
using MediatR;

namespace backend.Modules.Social.Handlers;

public class WorkoutCompletedSocialPostHandler : INotificationHandler<WorkoutCompletedEvent>
{
    private readonly ISocialRepository _socialRepository;
    private readonly IUnitOfWork _unitOfWork;

    public WorkoutCompletedSocialPostHandler(ISocialRepository socialRepository, IUnitOfWork unitOfWork)
    {
        _socialRepository = socialRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(WorkoutCompletedEvent notification, CancellationToken cancellationToken)
    {
        if (notification.IsPrivate)
            return;

        var existingPost = await _socialRepository.GetPostByReferenceAsync(
            PostType.WorkoutShare,
            notification.WorkoutId,
            cancellationToken);

        if (existingPost is not null)
            return;

        var caption = string.IsNullOrWhiteSpace(notification.Notes)
            ? BuildCaption(notification.WorkoutType)
            : notification.Notes.Trim();

        var post = Post.CreateWorkoutSharePost(
            notification.UserId,
            notification.WorkoutId,
            caption);

        await _socialRepository.AddPostAsync(post, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static string BuildCaption(string workoutType)
    {
        var displayName = string.IsNullOrWhiteSpace(workoutType)
            ? "workout"
            : workoutType.ToLowerInvariant();

        return $"Completed a {displayName} workout.";
    }
}
