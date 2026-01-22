namespace backend.Modules.User.Domain;

public class AppUserPreference
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }

    public string PreferredLanguage { get; set; } = "en";
    public bool IsDarkModeEnabled { get; set; } = false;
    public bool ReceiveEmailNotifications { get; set; } = true;

    public string UnitSystem { get; set; } = "metric"; // or 'imperial'
    public string TimeZoneId { get; set; } = "Central European Standard Time"; // Default to Polish time per requirements

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public AppUser AppUser { get; set; } = null!;
}