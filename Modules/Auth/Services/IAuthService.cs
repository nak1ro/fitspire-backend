using backend.Modules.Auth.DTOs;
using backend.Modules.User.Domain;

namespace backend.Modules.Auth.Services;

public interface IAuthService
{
    public Task<NewUserDto> RegisterAsync(RegisterDto dto);
    public Task<NewUserDto> LoginAsync(LoginDto dto);
    Task<bool> ConfirmEmailAsync(ConfirmEmailDto dto);
    Task<NewUserDto> ExternalLoginAsync(ExternalLoginDto dto);
    Task ForgotPasswordAsync(ForgotPasswordDto dto);
    Task ResetPasswordAsync(ResetPasswordDto dto);
    Task ChangePasswordAsync(Guid userId, ChangePasswordDto dto);
}
