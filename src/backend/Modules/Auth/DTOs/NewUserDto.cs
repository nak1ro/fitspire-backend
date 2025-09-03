namespace backend.Modules.Auth.DTOs;

public class NewUserDto
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public string Token { get; set; }
}
