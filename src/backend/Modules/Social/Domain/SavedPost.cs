using backend.Modules.User.Domain;

namespace backend.Modules.Social.Domain;

public class SavedPost
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }
    public Guid PostId { get; set; }
    public DateTime SavedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public AppUser User { get; set; } = null!;
    public Post Post { get; set; } = null!;
}