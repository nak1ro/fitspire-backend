using backend.Modules.User.Domain;

namespace backend.Modules.Moderation.Domain;

public class Report
{
    public Guid Id { get; set; }

    public Guid ReportedById { get; set; }
    public Guid ReportedUserId { get; set; }

    public string Reason { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public AppUser ReportedBy { get; set; } = null!;
    public AppUser ReportedUser { get; set; } = null!;
}