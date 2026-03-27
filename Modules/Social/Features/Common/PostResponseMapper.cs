using backend.Modules.Social.Domain;
using backend.Modules.Social.Domain.Enums;
using backend.Modules.Workout.Domain.Entities;
using backend.Modules.Workout.Infrastructure;

namespace backend.Modules.Social.Features.Common;

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
    IReadOnlyList<CommentPreviewResponse> RecentComments,
    DateTime CreatedAt
);

public record CommentPreviewResponse(
    Guid Id,
    Guid UserId,
    string UserName,
    string? UserAvatarUrl,
    string Content,
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

public static class PostResponseMapper
{
    public static async Task<List<FeedItemResponse>> MapAsync(
        List<Post> posts,
        Guid currentUserId,
        IWorkoutRepository workoutRepository,
        CancellationToken cancellationToken)
    {
        var workoutSummaries = await GetWorkoutSummariesAsync(posts, workoutRepository, cancellationToken);

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
            IsLikedByCurrentUser(p, currentUserId),
            p.Comments.Count,
            GetRecentComments(p),
            p.CreatedAt
        )).ToList();
    }

    private static async Task<Dictionary<Guid, WorkoutSummaryResponse>> GetWorkoutSummariesAsync(
        List<Post> posts,
        IWorkoutRepository workoutRepository,
        CancellationToken cancellationToken)
    {
        var workoutIds = posts
            .Where(p => p.Type == PostType.WorkoutShare && p.ReferenceEntityId.HasValue)
            .Select(p => p.ReferenceEntityId!.Value)
            .ToList();

        if (!workoutIds.Any())
            return new Dictionary<Guid, WorkoutSummaryResponse>();

        var workouts = await workoutRepository.GetByIdsAsync(workoutIds, cancellationToken);

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

    private static IReadOnlyList<CommentPreviewResponse> GetRecentComments(Post post)
    {
        return post.Comments
            .OrderByDescending(c => c.CreatedAt)
            .Take(2)
            .Select(c => new CommentPreviewResponse(
                c.Id,
                c.UserId,
                c.User?.UserName ?? "Unknown",
                c.User?.ProfilePictureUrl,
                c.Content,
                c.CreatedAt))
            .ToList();
    }
}
