using backend.Modules.User.Domain;

namespace backend.Modules.Social.Domain;

public class Like
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }
    public Guid TargetId { get; set; } // Post or Comment
    public string TargetType { get; set; } = null!; // 'post' | 'comment'
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public AppUser User { get; set; } = null!;
}