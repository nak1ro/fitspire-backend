using backend.Modules.User.Domain;

namespace backend.Modules.Auth.Services;

public interface ITokenService
{
    Task<string> CreateToken(AppUser appUser);
}