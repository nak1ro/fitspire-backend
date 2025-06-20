namespace backend.Modules.Auth.DTOs;

public class ConfirmEmailDto
{
    public Guid UserId { get; set; }
    public string Token { get; set; }
}