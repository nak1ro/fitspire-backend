using backend.Modules.User.Domain;

namespace backend.Modules.Social.Domain;

public class Post
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }
    public string Content { get; set; } = null!;
    public string? ImageUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Guid? CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }

    // Navigation
    public AppUser User { get; set; } = null!;
    public AppUser? Creator { get; set; }
    public AppUser? Updater { get; set; }

    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<Like> Likes { get; set; } = new List<Like>();
    public ICollection<SavedPost> SavedByUsers { get; set; } = new List<SavedPost>();
} 