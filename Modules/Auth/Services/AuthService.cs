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
        var user = new AppUser
        {
            UserName = dto.UserName,
            Email = dto.Email,
            DisplayName = dto.DisplayName
        };

        // Check for existing username/email
        if (await _userManager.FindByNameAsync(dto.UserName) != null)
            throw new InvalidOperationException("Username is already taken.");

        if (await _userManager.FindByEmailAsync(dto.Email) != null)
            throw new InvalidOperationException("Email is already taken.");

        var result = await _userManager.CreateAsync(user, dto.Password);

        if (!result.Succeeded)
        {
            var errorMessages = string.Join("; ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException(errorMessages);
        }

        await _userManager.AddToRoleAsync(user, "User");

        // Generate email confirmation token
        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var confirmationLink =
            $"https://your-frontend-app/confirm-email?userId={user.Id}&token={Uri.EscapeDataString(token)}";

        var emailHtml = $@"
                <p>Hello {user.DisplayName},</p>
                <p>Thanks for registering! Please confirm your email by clicking the link below:</p>
                <a href=""{confirmationLink}"">Confirm Email</a>
            ";
        
        await _emailService.SendMockEmailAsync(user.Email, "Confirm your Fitspire account", emailHtml);
        
        // Optionally return a flag that says "email confirmation required"
        return new NewUserDto
        {
            Id = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            CreatedAt = user.CreatedAt,
            Token = null // don't return a token before confirmation
        };
    }


    public async Task<NewUserDto> LoginAsync(LoginDto dto)
    {
        var user = await _userManager.Users
            .FirstOrDefaultAsync(u => u.UserName == dto.Login || u.Email == dto.Login);

        if (user == null)
            throw new UnauthorizedAccessException("Invalid username or email");

        // Require email confirmation
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
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            throw new UnauthorizedAccessException("Invalid username or email");

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
                EmailConfirmed = true, // Google verified
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