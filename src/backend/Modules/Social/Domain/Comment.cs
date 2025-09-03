using backend.Modules.User.Domain;

namespace backend.Modules.Social.Domain;

public class Comment
{
    public Guid Id { get; set; }

    public Guid PostId { get; set; }
    public Guid UserId { get; set; }

    public string Content { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Post Post { get; set; } = null!;
    public AppUser User { get; set; } = null!;
}