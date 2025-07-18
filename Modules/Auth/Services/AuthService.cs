using backend.Modules.Auth.DTOs;
using backend.Modules.User.Domain;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace backend.Modules.Auth.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly ITokenService _tokenService;
    private readonly IEmailService _emailService;

    public AuthService(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager,
        ITokenService tokenService, IEmailService emailService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenService = tokenService;
        _emailService = emailService;
    }

    public async Task<NewUserDto> RegisterAsync(RegisterDto dto)
    {
        // Check for existing username/email
        if (await _userManager.FindByNameAsync(dto.UserName) != null)
            throw new InvalidOperationException("Username is already taken.");

        if (await _userManager.FindByEmailAsync(dto.Email) != null)
            throw new InvalidOperationException("Email is already taken.");

        var user = new AppUser
        {
            UserName = dto.UserName,
            Email = dto.Email,
            DisplayName = string.IsNullOrWhiteSpace(dto.DisplayName) ? dto.UserName : dto.DisplayName,
            // Mark as confirmed by default
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, dto.Password);

        if (!result.Succeeded)
        {
            var errorMessages = string.Join("; ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException(errorMessages);
        }

        await _userManager.AddToRoleAsync(user, "User");

        // NOTE: We intentionally skip generating/sending any confirmation email.
        // If you want to keep a mock log, you could call:
        // await _emailService.SendMockEmailAsync(user.Email, "Confirm your Fitspire account", "Email auto-confirmed in dev.");

        return new NewUserDto
        {
            Id = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            CreatedAt = user.CreatedAt,
            Token = null // keeping current contract: no auto login on register
        };
    }

    public async Task<NewUserDto> LoginAsync(LoginDto dto)
    {
        var user = await _userManager.Users
            .FirstOrDefaultAsync(u => u.UserName == dto.Login || u.Email == dto.Login);

        if (user == null)
            throw new UnauthorizedAccessException("Invalid username or email");

        // Email is now confirmed by default for new users, but keep the check for older accounts.
        if (!user.EmailConfirmed)
            throw new UnauthorizedAccessException("Email not confirmed.");

        var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, false);
        if (!result.Succeeded)
            throw new UnauthorizedAccessException("Invalid credentials");

        return new NewUserDto
        {
            Id = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            CreatedAt = user.CreatedAt,
            Token = await _tokenService.CreateToken(user)
        };
    }

    public async Task<bool> ConfirmEmailAsync(Guid userId, string token)
    {
        // Since we auto-confirm, just succeed if the user exists.
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            throw new UnauthorizedAccessException("Invalid username or email");

        if (user.EmailConfirmed) return true;

        // For legacy users, you may still support token-based confirm:
        var result = await _userManager.ConfirmEmailAsync(user, token);
        return result.Succeeded;
    }

    public async Task<NewUserDto> ExternalLoginAsync(ExternalLoginDto dto)
    {
        if (dto.Provider != "Google")
            throw new InvalidOperationException("Unsupported provider.");

        // Validate the Google token (using Google API)
        var payload = await GoogleJsonWebSignature.ValidateAsync(dto.IdToken);

        // Find or create user
        var user = await _userManager.Users
            .FirstOrDefaultAsync(u => u.Email == payload.Email);

        if (user == null)
        {
            user = new AppUser
            {
                UserName = payload.Email,
                Email = payload.Email,
                EmailConfirmed = true, // Google verified (and we also default-confirm)
            };
            var result = await _userManager.CreateAsync(user);
            if (!result.Succeeded)
                throw new InvalidOperationException("User creation failed.");
            await _userManager.AddToRoleAsync(user, "User");
        }

        // Issue JWT
        return new NewUserDto
        {
            Id = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            CreatedAt = user.CreatedAt,
            Token = await _tokenService.CreateToken(user)
        };
    }
}
