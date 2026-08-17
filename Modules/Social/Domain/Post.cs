using backend.Modules.Shared.Domain;
using backend.Modules.Media.Domain;
using backend.Modules.Social.Domain.Enums;
using backend.Modules.User.Domain;

namespace backend.Modules.Social.Domain;

/// <summary>
/// Represents a social feed post. Can be text, a workout share, or a goal achievement.
/// </summary>
public class Post : Entity<Guid>
{
    public Guid UserId { get; private set; }
    public PostType Type { get; private set; }
    public string? Content { get; private set; }
    
    /// <summary>
    /// Reference to WorkoutId or GoalId when Type is WorkoutShare or GoalAchieved.
    /// </summary>
    public Guid? ReferenceEntityId { get; private set; }
    public DateTime? ModerationRemovedAtUtc { get; private set; }
    public bool IsModerationRemoved => ModerationRemovedAtUtc is not null;

    // Navigation
    public AppUser User { get; private set; } = null!;
    public ICollection<Comment> Comments { get; private set; } = new List<Comment>();
    public WorkoutShareSnapshot? WorkoutShareSnapshot { get; private set; }
    public GoalAchievedSnapshot? GoalAchievedSnapshot { get; private set; }
    public ICollection<PostLike> Likes { get; private set; } = new List<PostLike>();
    public ICollection<SavedPost> SavedByUsers { get; private set; } = new List<SavedPost>();
    public ICollection<PostMedia> Media { get; private set; } = new List<PostMedia>();

    private Post() { }

    /// <summary>
    /// Create a text post.
    /// </summary>
    public static Post CreateTextPost(Guid userId, string? content, IReadOnlyList<Guid>? mediaAssetIds = null)
    {
        var post = new Post
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Type = PostType.Text,
            CreatedAt = DateTime.UtcNow
        };

        post.UpdateTextPost(content, mediaAssetIds);
        return post;
    }

    /// <summary>
    /// Create a workout share post (auto-generated when workout is completed).
    /// </summary>
    public static Post CreateWorkoutSharePost(Guid userId, Guid workoutId, string? caption = null)
    {
        return new Post
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Type = PostType.WorkoutShare,
            Content = caption,
            ReferenceEntityId = workoutId,
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Create a goal achieved post (auto-generated when goal is completed).
    /// </summary>
    public static Post CreateGoalAchievedPost(Guid userId, Guid goalId, string? caption = null)
    {
        return new Post
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Type = PostType.GoalAchieved,
            Content = caption,
            ReferenceEntityId = goalId,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static Post CreateGoalAchievedPost(
        Guid userId,
        GoalAchievedSnapshot snapshot,
        string? caption = null,
        IReadOnlyList<Guid>? mediaAssetIds = null)
    {
        var post = CreateGoalAchievedPost(userId, snapshot.SourceGoalId, caption);
        post.GoalAchievedSnapshot = snapshot;

        if (mediaAssetIds is { Count: > 0 })
            post.ApplyMediaSet(mediaAssetIds);

        return post;
    }

    public static Post CreateWorkoutSharePost(
        Guid userId,
        WorkoutShareSnapshot snapshot,
        string? caption = null,
        IReadOnlyList<Guid>? mediaAssetIds = null)
    {
        var post = CreateWorkoutSharePost(userId, snapshot.SourceWorkoutId, caption);
        post.WorkoutShareSnapshot = snapshot;

        if (mediaAssetIds is { Count: > 0 })
            post.ApplyMediaSet(mediaAssetIds);

        return post;
    }

    public void UpdateTextPost(string? content, IReadOnlyList<Guid>? mediaAssetIds)
    {
        if (Type != PostType.Text)
        {
            throw new DomainException("Only text posts can be edited.");
        }

        if (content is not null)
            Content = NormalizeContent(content);

        if (mediaAssetIds is not null)
            ApplyMediaSet(mediaAssetIds);

        EnsurePublishable();
        UpdatedAt = DateTime.UtcNow;
    }

    public void RemoveByModeration(DateTime utcNow)
    {
        if (utcNow.Kind != DateTimeKind.Utc)
            throw new DomainException("Moderation removal time must be in UTC.");
        if (ModerationRemovedAtUtc is not null)
            return;

        ModerationRemovedAtUtc = utcNow;
        UpdatedAt = utcNow;
    }

    public void RestoreByModeration(DateTime utcNow)
    {
        if (utcNow.Kind != DateTimeKind.Utc)
            throw new DomainException("Moderation restoration time must be in UTC.");
        if (ModerationRemovedAtUtc is null)
            return;

        ModerationRemovedAtUtc = null;
        UpdatedAt = utcNow;
    }

    private void ApplyMediaSet(IReadOnlyList<Guid> mediaAssetIds)
    {
        if (mediaAssetIds.Count > MediaPolicies.MaximumPostImages)
            throw new DomainException("A post can contain at most ten images.");

        if (mediaAssetIds.Any(id => id == Guid.Empty) || mediaAssetIds.Distinct().Count() != mediaAssetIds.Count)
            throw new DomainException("Post media IDs must be unique and non-empty.");

        var currentMedia = Media.ToDictionary(media => media.MediaAssetId);
        var requestedMedia = mediaAssetIds.ToHashSet();

        foreach (var media in Media.Where(media => !requestedMedia.Contains(media.MediaAssetId)).ToList())
            Media.Remove(media);

        for (var order = 0; order < mediaAssetIds.Count; order++)
        {
            var mediaAssetId = mediaAssetIds[order];
            if (currentMedia.TryGetValue(mediaAssetId, out var existing))
            {
                existing.MoveTo(order);
                continue;
            }

            Media.Add(PostMedia.Create(Id, mediaAssetId, order));
        }
    }

    private void EnsurePublishable()
    {
        if (string.IsNullOrWhiteSpace(Content) && Media.Count == 0)
            throw new DomainException("A post needs text or at least one image.");
    }

    private static string? NormalizeContent(string content)
    {
        var normalized = content.Trim();
        if (normalized.Length > 2000)
            throw new DomainException("Post content must be at most 2000 characters.");

        return string.IsNullOrEmpty(normalized) ? null : normalized;
    }
}
