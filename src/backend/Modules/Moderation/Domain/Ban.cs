using backend.Modules.User.Domain;

namespace backend.Modules.Moderation.Domain;

public class Ban
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string Reason { get; set; } = null!;
    public DateTime? BannedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }

    // Navigation
    public AppUser User { get; set; } = null!;
}