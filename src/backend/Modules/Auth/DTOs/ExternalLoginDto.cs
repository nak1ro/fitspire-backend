namespace backend.Modules.Auth.DTOs;

public class ExternalLoginDto
{
    public string Provider { get; set; }  // e.g., "Google"
    public string IdToken { get; set; }
}
