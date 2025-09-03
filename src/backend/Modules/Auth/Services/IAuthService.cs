using backend.Modules.Auth.DTOs;
using backend.Modules.User.Domain;

namespace backend.Modules.Auth.Services;

public interface IAuthService
{
    public Task<NewUserDto> RegisterAsync(RegisterDto dto);
    public Task<NewUserDto> LoginAsync(LoginDto dto);
    Task<bool> ConfirmEmailAsync(Guid userId, string token);
    Task<NewUserDto> ExternalLoginAsync(ExternalLoginDto dto);
}