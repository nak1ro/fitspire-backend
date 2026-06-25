namespace backend.Modules.User.DTOs;

public class UserPreferencesDto
{
    public string PreferredLanguage { get; set; } = "en";
    public bool IsDarkModeEnabled { get; set; }
    public bool ReceiveEmailNotifications { get; set; }
    public string UnitSystem { get; set; } = "metric";
    public string TimeZoneId { get; set; } = "Central European Standard Time";
}
