using backend.Modules.Social.Domain;
using backend.Modules.Social.Domain.Enums;
using backend.Modules.Media.Contracts;
using backend.Modules.Workout.Domain.Entities;
using backend.Modules.Workout.Infrastructure;

namespace backend.Modules.Social.Features.Common;

public record FeedItemResponse(
    Guid Id,
    Guid UserId,
    string UserName,
    string? UserAvatarUrl,
    MediaResponse? UserAvatar,
    string Type,
    string? Content,
    Guid? ReferenceEntityId,
    WorkoutSummaryResponse? WorkoutSummary,
    IReadOnlyList<MediaResponse> Media,
    int LikesCount,
    bool IsLikedByCurrentUser,
    bool IsSavedByCurrentUser,
    int CommentsCount,
    IReadOnlyList<CommentPreviewResponse> RecentComments,
    DateTime CreatedAt
);

public record CommentPreviewResponse(
    Guid Id,
    Guid UserId,
    string UserName,
    string? UserAvatarUrl,
    MediaResponse? UserAvatar,
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
    int? ExerciseCount,
    DateTime? CompletedAt
);

public static class PostResponseMapper
{
    public static async Task<List<FeedItemResponse>> MapAsync(
        List<Post> posts,
        Guid currentUserId,
        IWorkoutRepository workoutRepository,
        IMediaResponseFactory mediaResponseFactory,
        CancellationToken cancellationToken)
    {
        var workoutSummaries = await GetWorkoutSummariesAsync(posts, workoutRepository, cancellationToken);
        var mediaResponses = await GetMediaResponsesAsync(posts, mediaResponseFactory, cancellationToken);

        return posts.Select(p => new FeedItemResponse(
            p.Id,
            p.UserId,
            p.User?.UserName ?? "Unknown",
            GetAvatarUrl(p.User?.ProfilePictureMedia, mediaResponses),
            GetMediaResponse(p.User?.ProfilePictureMedia, mediaResponses),
            p.Type.ToString(),
            p.Content,
            p.ReferenceEntityId,
            GetWorkoutSummary(p, workoutSummaries),
            GetPostMedia(p, mediaResponses),
            p.Likes.Count,
            IsLikedByCurrentUser(p, currentUserId),
            IsSavedByCurrentUser(p, currentUserId),
            p.Comments.Count,
            GetRecentComments(p, mediaResponses),
            p.CreatedAt
        )).ToList();
    }

    private static async Task<Dictionary<Guid, WorkoutSummaryResponse>> GetWorkoutSummariesAsync(
        List<Post> posts,
        IWorkoutRepository workoutRepository,
        CancellationToken cancellationToken)
    {
        var workoutIds = posts
            .Where(p => p.Type == PostType.WorkoutShare
                        && p.WorkoutShareSnapshot is null
                        && p.ReferenceEntityId.HasValue)
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
        if (post.Type != PostType.WorkoutShare)
            return null;

        if (post.WorkoutShareSnapshot is not null)
        {
            var snapshot = post.WorkoutShareSnapshot;
            return new WorkoutSummaryResponse(
                snapshot.SourceWorkoutId,
                snapshot.WorkoutType,
                snapshot.WorkoutDate,
                snapshot.DurationMinutes,
                snapshot.DistanceKm,
                snapshot.CaloriesBurned,
                snapshot.TotalVolumeKg,
                snapshot.ExerciseCount,
                snapshot.CompletedAt);
        }

        if (!post.ReferenceEntityId.HasValue)
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
            workout.GetExerciseCount(),
            workout.CompletedAt
        );
    }

    private static bool IsLikedByCurrentUser(Post post, Guid userId)
    {
        return post.Likes.Any(l => l.UserId == userId);
    }

    private static bool IsSavedByCurrentUser(Post post, Guid userId)
    {
        return post.SavedByUsers.Any(saved => saved.UserId == userId);
    }

    private static IReadOnlyList<CommentPreviewResponse> GetRecentComments(
        Post post,
        IReadOnlyDictionary<Guid, MediaResponse> responses)
    {
        return post.Comments
            .OrderByDescending(c => c.CreatedAt)
            .Take(2)
            .Select(c => new CommentPreviewResponse(
                c.Id,
                c.UserId,
                c.User?.UserName ?? "Unknown",
                GetAvatarUrl(c.User?.ProfilePictureMedia, responses),
                GetMediaResponse(c.User?.ProfilePictureMedia, responses),
                c.Content,
                c.CreatedAt))
            .ToList();
    }

    private static async Task<IReadOnlyDictionary<Guid, MediaResponse>> GetMediaResponsesAsync(
        IEnumerable<Post> posts,
        IMediaResponseFactory mediaResponseFactory,
        CancellationToken cancellationToken)
    {
        var assets = posts.SelectMany(post => post.Media.Select(media => media.MediaAsset))
            .Concat(posts.Select(post => post.User?.ProfilePictureMedia).OfType<backend.Modules.Media.Domain.MediaAsset>())
            .Concat(posts.SelectMany(post => post.Comments)
                .Select(comment => comment.User?.ProfilePictureMedia)
                .OfType<backend.Modules.Media.Domain.MediaAsset>())
            .ToList();
        return await mediaResponseFactory.CreateManyAsync(assets, cancellationToken);
    }

    private static IReadOnlyList<MediaResponse> GetPostMedia(
        Post post,
        IReadOnlyDictionary<Guid, MediaResponse> responses)
    {
        return post.Media
            .OrderBy(media => media.Order)
            .Select(media => responses.GetValueOrDefault(media.MediaAssetId))
            .Where(response => response is not null)
            .Cast<MediaResponse>()
            .ToList();
    }

    private static MediaResponse? GetMediaResponse(
        backend.Modules.Media.Domain.MediaAsset? asset,
        IReadOnlyDictionary<Guid, MediaResponse> responses) =>
        asset is null ? null : responses.GetValueOrDefault(asset.Id);

    private static string? GetAvatarUrl(
        backend.Modules.Media.Domain.MediaAsset? asset,
        IReadOnlyDictionary<Guid, MediaResponse> responses) =>
        GetMediaResponse(asset, responses)?.Thumbnail?.Url;
}
