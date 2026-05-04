using backend.Modules.Auth.DTOs;
using FluentValidation;
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
    private readonly IValidator<RegisterDto> _registerValidator;
    private readonly IValidator<LoginDto> _loginValidator;
    private readonly IValidator<ConfirmEmailDto> _confirmEmailValidator;
    private readonly IValidator<ExternalLoginDto> _externalLoginValidator;
    private readonly IValidator<ChangePasswordDto> _changePasswordValidator;
    private readonly IValidator<ForgotPasswordDto> _forgotPasswordValidator;
    private readonly IValidator<ResetPasswordDto> _resetPasswordValidator;
    private readonly string _frontendBaseUrl;
    private readonly bool _useMockEmail;

    public AuthService(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager,
        ITokenService tokenService, IEmailService emailService, IValidator<RegisterDto> registerValidator,
        IValidator<LoginDto> loginValidator,
        IValidator<ConfirmEmailDto> confirmEmailValidator,
        IValidator<ExternalLoginDto> externalLoginValidator,
        IValidator<ChangePasswordDto> changePasswordValidator,
        IValidator<ForgotPasswordDto> forgotPasswordValidator,
        IConfiguration configuration,
        IValidator<ResetPasswordDto> resetPasswordValidator)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenService = tokenService;
        _emailService = emailService;
        _registerValidator = registerValidator;
        _loginValidator = loginValidator;
        _confirmEmailValidator = confirmEmailValidator;
        _externalLoginValidator = externalLoginValidator;
        _changePasswordValidator = changePasswordValidator;
        _forgotPasswordValidator = forgotPasswordValidator;
        _resetPasswordValidator = resetPasswordValidator;
        _frontendBaseUrl = GetFrontendBaseUrl(configuration);
        _useMockEmail = configuration.GetValue("Email:UseMockEmail", true);
    }

    public async Task<NewUserDto> RegisterAsync(RegisterDto dto)
    {
        await _registerValidator.ValidateAndThrowAsync(dto);

        // Check for existing username/email
        if (await _userManager.FindByNameAsync(dto.UserName) != null)
            throw new InvalidOperationException("Username is already taken.");

        if (await _userManager.FindByEmailAsync(dto.Email) != null)
            throw new InvalidOperationException("Email is already taken.");
        
        var user = new AppUser
        {
            UserName = dto.UserName,
            Email = dto.Email,
            DisplayName = string.IsNullOrWhiteSpace(dto.DisplayName) ? dto.UserName : dto.DisplayName
        };
        
        var result = await _userManager.CreateAsync(user, dto.Password);

        if (!result.Succeeded)
        {
            var errorMessages = string.Join("; ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException(errorMessages);
        }

        await _userManager.AddToRoleAsync(user, "User");

        // Generate email confirmation token
        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var confirmationLink = BuildFrontendLink(
            "confirm-email",
            ("userId", user.Id.ToString()),
            ("token", token));

        var emailHtml = $@"
                <p>Hello {user.DisplayName},</p>
                <p>Thanks for registering! Please confirm your email by clicking the link below:</p>
                <a href=""{confirmationLink}"">Confirm Email</a>
            ";
        
        await SendAccountEmailAsync(RequireEmail(user), "Confirm your Fitspire account", emailHtml);

        return CreateNewUserDto(user, null);
    }


    public async Task<NewUserDto> LoginAsync(LoginDto dto)
    {
        await _loginValidator.ValidateAndThrowAsync(dto);

        var user = await _userManager.Users
            .FirstOrDefaultAsync(u => u.UserName == dto.Login || u.Email == dto.Login);

        if (user == null)
            throw new UnauthorizedAccessException("Invalid username or email");

        var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, false);
        if (!result.Succeeded)
            throw new UnauthorizedAccessException("Invalid credentials");

        return CreateNewUserDto(user, await _tokenService.CreateToken(user));
    }

    public async Task<bool> ConfirmEmailAsync(ConfirmEmailDto dto)
    {
        await _confirmEmailValidator.ValidateAndThrowAsync(dto);

        var user = await _userManager.FindByIdAsync(dto.UserId.ToString());
        if (user == null)
            throw new UnauthorizedAccessException("Invalid username or email");

        var result = await _userManager.ConfirmEmailAsync(user, dto.Token);
        return result.Succeeded;
    }

    public async Task<NewUserDto> ExternalLoginAsync(ExternalLoginDto dto)
    {
        await _externalLoginValidator.ValidateAndThrowAsync(dto);

        if (!dto.Provider.Equals("Google", StringComparison.OrdinalIgnoreCase))
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
        return CreateNewUserDto(user, await _tokenService.CreateToken(user));
    }

    public async Task ForgotPasswordAsync(ForgotPasswordDto dto)
    {
        await _forgotPasswordValidator.ValidateAndThrowAsync(dto);

        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null || !user.EmailConfirmed)
            return;

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var resetLink = BuildFrontendLink(
            "reset-password",
            ("email", user.Email!),
            ("token", token));

        var emailHtml = $@"
                <p>Hello {user.DisplayName},</p>
                <p>You requested to reset your password. Click the link below to continue:</p>
                <a href=""{resetLink}"">Reset Password</a>
            ";

        await SendAccountEmailAsync(user.Email!, "Reset your Fitspire password", emailHtml);
    }

    public async Task ChangePasswordAsync(Guid userId, ChangePasswordDto dto)
    {
        await _changePasswordValidator.ValidateAndThrowAsync(dto);

        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            throw new UnauthorizedAccessException("User not found.");

        var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
        if (!result.Succeeded)
        {
            var errorMessages = string.Join("; ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException(string.IsNullOrWhiteSpace(errorMessages)
                ? "Password change failed."
                : errorMessages);
        }
    }

    public async Task ResetPasswordAsync(ResetPasswordDto dto)
    {
        await _resetPasswordValidator.ValidateAndThrowAsync(dto);

        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null)
            throw new InvalidOperationException("Invalid reset request.");

        var result = await _userManager.ResetPasswordAsync(user, dto.Token, dto.NewPassword);
        if (!result.Succeeded)
        {
            var errorMessages = string.Join("; ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException(string.IsNullOrWhiteSpace(errorMessages)
                ? "Invalid reset request."
                : errorMessages);
        }
    }

    private string BuildFrontendLink(string path, params (string Key, string Value)[] queryParameters)
    {
        var normalizedPath = path.TrimStart('/');
        var query = string.Join(
            "&",
            queryParameters.Select(parameter =>
                $"{Uri.EscapeDataString(parameter.Key)}={Uri.EscapeDataString(parameter.Value)}"));

        return $"{_frontendBaseUrl}/{normalizedPath}?{query}";
    }

    private Task SendAccountEmailAsync(string to, string subject, string htmlContent)
    {
        return _useMockEmail
            ? _emailService.SendMockEmailAsync(to, subject, htmlContent)
            : _emailService.SendEmailAsync(to, subject, htmlContent);
    }

    private static NewUserDto CreateNewUserDto(AppUser user, string? token)
    {
        return new NewUserDto
        {
            Id = user.Id,
            UserName = RequireUserName(user),
            Email = RequireEmail(user),
            CreatedAt = user.CreatedAt,
            Token = token
        };
    }

    private static string RequireUserName(AppUser user)
    {
        return user.UserName ?? throw new InvalidOperationException("Username is required for auth responses.");
    }

    private static string RequireEmail(AppUser user)
    {
        return user.Email ?? throw new InvalidOperationException("Email is required for auth responses.");
    }

    private static string GetFrontendBaseUrl(IConfiguration configuration)
    {
        var baseUrl = configuration["Frontend:BaseUrl"];
        if (string.IsNullOrWhiteSpace(baseUrl))
            throw new InvalidOperationException("Frontend:BaseUrl configuration is required for account email links.");

        return baseUrl.Trim().TrimEnd('/');
    }
}
