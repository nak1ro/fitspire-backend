namespace backend.Modules.Auth.DTOs;

public class ExternalLoginDto
{
    public string Provider { get; set; } = string.Empty; // e.g., "Google"
    public string IdToken { get; set; } = string.Empty;
}
