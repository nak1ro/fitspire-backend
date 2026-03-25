using backend.Modules.Social.Domain;
using backend.Modules.Social.Domain.Enums;
using backend.Modules.Social.Infrastructure;
using backend.Modules.Shared;
using backend.Modules.Workout.Domain.Entities;
using backend.Modules.Workout.Infrastructure;
using MediatR;

namespace backend.Modules.Social.Features.Feed;

public record GetUserFeedQuery(Guid UserId, int Page = 1, int PageSize = 20) : IRequest<List<FeedItemResponse>>;

public record FeedItemResponse(
    Guid Id,
    Guid UserId,
    string UserName,
    string? UserAvatarUrl,
    string Type,
    string? Content,
    Guid? ReferenceEntityId,
    WorkoutSummaryResponse? WorkoutSummary,
    int LikesCount,
    bool IsLikedByCurrentUser,
    int CommentsCount,
    DateTime CreatedAt
);

public record WorkoutSummaryResponse(
    Guid Id,
    string WorkoutType,
    DateTime Date,
    double? DurationMinutes,
    double? DistanceKm,
    int? CaloriesBurned,
    double? TotalVolumeKg,
    DateTime? CompletedAt
);

public class GetUserFeedHandler : IRequestHandler<GetUserFeedQuery, List<FeedItemResponse>>
{
    private readonly ISocialRepository _socialRepository;
    private readonly IWorkoutRepository _workoutRepository;

    public GetUserFeedHandler(ISocialRepository socialRepository, IWorkoutRepository workoutRepository)
    {
        _socialRepository = socialRepository;
        _workoutRepository = workoutRepository;
    }

    public async Task<List<FeedItemResponse>> Handle(GetUserFeedQuery request, CancellationToken cancellationToken)
    {
        var posts = await _socialRepository.GetUserFeedAsync(request.UserId, request.Page, request.PageSize, cancellationToken);
        var workoutSummaries = await GetWorkoutSummariesAsync(posts, cancellationToken);

        return posts.Select(p => new FeedItemResponse(
            p.Id,
            p.UserId,
            p.User?.UserName ?? "Unknown",
            p.User?.ProfilePictureUrl,
            p.Type.ToString(),
            p.Content,
            p.ReferenceEntityId,
            GetWorkoutSummary(p, workoutSummaries),
            p.Likes.Count,
            IsLikedByCurrentUser(p, request.UserId),
            p.Comments.Count,
            p.CreatedAt
        )).ToList();
    }

    private async Task<Dictionary<Guid, WorkoutSummaryResponse>> GetWorkoutSummariesAsync(
        List<Post> posts,
        CancellationToken cancellationToken)
    {
        var workoutIds = posts
            .Where(p => p.Type == PostType.WorkoutShare && p.ReferenceEntityId.HasValue)
            .Select(p => p.ReferenceEntityId!.Value)
            .ToList();

        if (!workoutIds.Any())
            return new Dictionary<Guid, WorkoutSummaryResponse>();

        var workouts = await _workoutRepository.GetByIdsAsync(workoutIds, cancellationToken);

        return workouts.ToDictionary(w => w.Id, MapWorkoutSummary);
    }

    private static WorkoutSummaryResponse? GetWorkoutSummary(
        Post post,
        Dictionary<Guid, WorkoutSummaryResponse> summaries)
    {
        if (post.Type != PostType.WorkoutShare || !post.ReferenceEntityId.HasValue)
            return null;

        return summaries.GetValueOrDefault(post.ReferenceEntityId.Value);
    }

    private static WorkoutSummaryResponse MapWorkoutSummary(UserWorkout workout)
    {
        return new WorkoutSummaryResponse(
            workout.Id,
            workout.WorkoutType,
            workout.Date,
            workout.DurationMinutes,
            workout.GetTotalDistance(),
            workout.CaloriesBurned,
            workout.GetTotalVolume(),
            workout.CompletedAt
        );
    }

    private static bool IsLikedByCurrentUser(Post post, Guid userId)
    {
        return post.Likes.Any(l => l.UserId == userId && l.TargetType == LikeTargetType.Post);
    }
}
