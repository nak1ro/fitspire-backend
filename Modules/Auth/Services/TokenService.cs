using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using backend.Modules.User.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace backend.Modules.Auth.Services;

public class TokenService : ITokenService
{
    private readonly SymmetricSecurityKey _key;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly UserManager<AppUser> _userManager;

    public TokenService(IConfiguration config, UserManager<AppUser> userManager)
    {
        _key = CreateSigningKey(GetRequiredConfig(config, "JWT:SigningKey"));
        _issuer = GetRequiredConfig(config, "JWT:Issuer");
        _audience = GetRequiredConfig(config, "JWT:Audience");
        _userManager = userManager;
    }

    public async Task<string> CreateToken(AppUser user)
    {
        var userRoles = await _userManager.GetRolesAsync(user);
        var email = user.Email ?? throw new InvalidOperationException("User email is required to create a token.");
        var userName = user.UserName ?? throw new InvalidOperationException("Username is required to create a token.");

        var claims = new List<Claim>()
        {
            new(JwtRegisteredClaimNames.Email, email),
            new(JwtRegisteredClaimNames.GivenName, userName),
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString())  
        };

        claims.AddRange(userRoles.Select(role => new Claim(ClaimTypes.Role, role)));

        var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256Signature);

        var tokenDescriptor = new SecurityTokenDescriptor()
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddDays(7),
            SigningCredentials = creds,
            Issuer = _issuer,
            Audience = _audience
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }

    private static string GetRequiredConfig(IConfiguration configuration, string key)
    {
        var value = configuration[key];
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidOperationException($"{key} configuration is required.");

        return value;
    }

    private static SymmetricSecurityKey CreateSigningKey(string signingKey)
    {
        if (Encoding.UTF8.GetByteCount(signingKey) < 32)
            throw new InvalidOperationException("JWT:SigningKey must be at least 32 bytes long.");

        return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey));
    }
}
