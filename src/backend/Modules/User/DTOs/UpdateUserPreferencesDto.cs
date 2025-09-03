namespace backend.Modules.User.DTOs;

public class UpdateUserPreferencesDto
{
    public string? PreferredLanguage { get; set; }
    public bool? IsDarkModeEnabled { get; set; }
    public bool? ReceiveEmailNotifications { get; set; }
    public string? UnitSystem { get; set; } // "metric" or "imperial"
}