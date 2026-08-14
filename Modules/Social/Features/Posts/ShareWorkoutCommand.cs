using backend.Data;
using backend.Modules.Badge.Services;
using backend.Modules.Shared;
using backend.Modules.Shared.Domain;
using backend.Modules.Social.Domain;
using backend.Modules.Social.Domain.Enums;
using backend.Modules.Social.Infrastructure;
using backend.Modules.Workout.Domain.Enums;
using backend.Modules.Workout.Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace backend.Modules.Social.Features.Posts;

public record ShareWorkoutCommand(Guid UserId, Guid WorkoutId, string? Caption = null) : IRequest<Guid>;

public class ShareWorkoutHandler : IRequestHandler<ShareWorkoutCommand, Guid>
{
    private readonly FitspireDbContext _context;
    private readonly IWorkoutRepository _workoutRepository;
    private readonly ISocialRepository _socialRepository;
    private readonly IBadgeEvaluationService _badges;
    private readonly IBadgeTransactionService _badgeTransactions;
    private readonly IUnitOfWork _unitOfWork;

    public ShareWorkoutHandler(
        FitspireDbContext context,
        IWorkoutRepository workoutRepository,
        ISocialRepository socialRepository,
        IBadgeEvaluationService badges,
        IBadgeTransactionService badgeTransactions,
        IUnitOfWork unitOfWork)
    {
        _context = context;
        _workoutRepository = workoutRepository;
        _socialRepository = socialRepository;
        _badges = badges;
        _badgeTransactions = badgeTransactions;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(ShareWorkoutCommand request, CancellationToken cancellationToken)
    {
        var workout = (await _workoutRepository.GetByIdsAsync([request.WorkoutId], cancellationToken)).SingleOrDefault();
        if (workout is null || workout.UserId != request.UserId)
            throw new NotFoundException($"Workout {request.WorkoutId} not found.");

        if (workout.Status != WorkoutStatus.Completed)
            throw new DomainException("Only completed workouts can be shared.");

        if (workout.IsPrivate)
            throw new DomainException("Private workouts cannot be shared.");

        var existingPost = await _socialRepository.GetPostByReferenceAsync(
            PostType.WorkoutShare,
            request.WorkoutId,
            cancellationToken);
        if (existingPost is not null)
        {
            if (existingPost.IsModerationRemoved)
                throw new ConflictException("The existing workout share is unavailable.");

            await _badges.EvaluateAsync(request.UserId, [BadgeTriggerContext.ForSocialPost(existingPost.Id)], cancellationToken);
            return existingPost.Id;
        }

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

        try
        {
            await _badgeTransactions.ExecuteAsync(async token =>
            {
                await _socialRepository.AddPostAsync(post, token);
                await _unitOfWork.SaveChangesAsync(token);
                await _badges.EvaluateAsync(request.UserId, [BadgeTriggerContext.ForSocialPost(post.Id)], token);
                await _unitOfWork.SaveChangesAsync(token);
            }, cancellationToken);
        }
        catch (DbUpdateException exception) when (IsUniqueViolation(exception))
        {
            _context.Entry(post).State = EntityState.Detached;
            var concurrentPost = await _socialRepository.GetPostByReferenceAsync(
                PostType.WorkoutShare,
                request.WorkoutId,
                cancellationToken);
            if (concurrentPost is not null)
            {
                if (concurrentPost.IsModerationRemoved)
                    throw new ConflictException("The existing workout share is unavailable.");

                await _badges.EvaluateAsync(request.UserId, [BadgeTriggerContext.ForSocialPost(concurrentPost.Id)], cancellationToken);
                return concurrentPost.Id;
            }

            throw;
        }

        return post.Id;
    }

    private static bool IsUniqueViolation(DbUpdateException exception)
    {
        return exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation };
    }
}
