using backend.Modules.User.Services;
using backend.Modules.User.DTOs;
using backend.Modules.User.Validators;
using FluentValidation;

namespace backend.Modules.User;

public static class UserModuleExtensions
{
    public static IServiceCollection AddUserModule(this IServiceCollection services)
    {
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IValidator<UpdateProfileDto>, UpdateProfileDtoValidator>();
        services.AddScoped<IValidator<AttachProfilePictureDto>, AttachProfilePictureDtoValidator>();
        services.AddScoped<IValidator<UpdateUserPreferencesDto>, UpdateUserPreferencesDtoValidator>();
        return services;
    }
}
