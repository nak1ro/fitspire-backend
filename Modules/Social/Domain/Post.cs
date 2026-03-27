using backend.Modules.Shared.Domain;
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
    public string? ImageUrl { get; private set; }
    
    /// <summary>
    /// Reference to WorkoutId or GoalId when Type is WorkoutShare or GoalAchieved.
    /// </summary>
    public Guid? ReferenceEntityId { get; private set; }

    // Navigation
    public AppUser User { get; private set; } = null!;
    public ICollection<Comment> Comments { get; private set; } = new List<Comment>();
    public ICollection<Like> Likes { get; private set; } = new List<Like>();
    public ICollection<SavedPost> SavedByUsers { get; private set; } = new List<SavedPost>();

    private Post() { }

    /// <summary>
    /// Create a text post.
    /// </summary>
    public static Post CreateTextPost(Guid userId, string content, string? imageUrl = null)
    {
        return new Post
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Type = PostType.Text,
            Content = content,
            ImageUrl = imageUrl,
            CreatedAt = DateTime.UtcNow
        };
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

    public void UpdateTextPost(string content, string? imageUrl = null)
    {
        if (Type != PostType.Text)
        {
            throw new DomainException("Only text posts can be edited.");
        }

        if (string.IsNullOrWhiteSpace(content))
        {
            throw new DomainException("Post content is required.");
        }

        Content = content.Trim();
        ImageUrl = string.IsNullOrWhiteSpace(imageUrl) ? null : imageUrl;
        UpdatedAt = DateTime.UtcNow;
    }
}
