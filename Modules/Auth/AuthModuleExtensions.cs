using System.Text;
using System.Security.Claims;
using backend.Modules.Auth.Authorization;
using backend.Modules.Auth.Services;
using backend.Modules.Auth.DTOs;
using backend.Modules.User.Domain;
using backend.Modules.Auth.Validators;
using FluentValidation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Identity;

namespace backend.Modules.Auth;

public static class AuthModuleExtensions
{
    public static IServiceCollection AddAuthModule(this IServiceCollection services, IConfiguration configuration)
    {
        var issuer = GetRequiredConfig(configuration, "JWT:Issuer");
        var audience = GetRequiredConfig(configuration, "JWT:Audience");
        var signingKey = CreateSigningKey(GetRequiredConfig(configuration, "JWT:SigningKey"));

        var authenticationBuilder = services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = issuer,

                    ValidateAudience = true,
                    ValidAudience = audience,

                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = signingKey,

                    ValidateLifetime = true
                };
                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = async context =>
                    {
                        var userIdClaim = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
                        if (!Guid.TryParse(userIdClaim, out var userId))
                        {
                            context.Fail("User identity is invalid.");
                            return;
                        }

                        var userManager = context.HttpContext.RequestServices.GetRequiredService<UserManager<AppUser>>();
                        var user = await userManager.FindByIdAsync(userId.ToString());
                        if (user is null || user.IsSuspended(DateTime.UtcNow))
                            context.Fail("Account access is unavailable.");
                    }
                };
            });

        AddGoogleIfConfigured(authenticationBuilder, configuration);

        services.AddAuthorization(options =>
        {
            options.AddPolicy(AppPolicies.AdminOnly, policy => policy.RequireRole(AppRoles.Admin));
        });

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IEmailService, ResendEmailService>();
        services.AddScoped<IValidator<RegisterDto>, RegisterDtoValidator>();
        services.AddScoped<IValidator<LoginDto>, LoginDtoValidator>();
        services.AddScoped<IValidator<ConfirmEmailDto>, ConfirmEmailDtoValidator>();
        services.AddScoped<IValidator<ExternalLoginDto>, ExternalLoginDtoValidator>();
        services.AddScoped<IValidator<ChangePasswordDto>, ChangePasswordDtoValidator>();
        services.AddScoped<IValidator<ForgotPasswordDto>, ForgotPasswordDtoValidator>();
        services.AddScoped<IValidator<ResetPasswordDto>, ResetPasswordDtoValidator>();

        return services;
    }

    private static void AddGoogleIfConfigured(AuthenticationBuilder authenticationBuilder, IConfiguration configuration)
    {
        var clientId = configuration["Authentication:Google:ClientId"];
        var clientSecret = configuration["Authentication:Google:ClientSecret"];

        if (string.IsNullOrWhiteSpace(clientId) && string.IsNullOrWhiteSpace(clientSecret))
            return;

        if (string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(clientSecret))
            throw new InvalidOperationException("Both Authentication:Google:ClientId and Authentication:Google:ClientSecret must be configured.");

        authenticationBuilder.AddGoogle(options =>
        {
            options.ClientId = clientId;
            options.ClientSecret = clientSecret;
        });
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
