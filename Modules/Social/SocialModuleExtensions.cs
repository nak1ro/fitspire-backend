using backend.Modules.Social.Infrastructure;
using backend.Modules.Social.Validators;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace backend.Modules.Social;

public static class SocialModuleExtensions
{
    public static IServiceCollection AddSocialModule(this IServiceCollection services)
    {
        services.AddScoped<ISocialRepository, SocialRepository>();
        services.AddScoped<IValidator<CreatePostRequest>, CreatePostRequestValidator>();
        services.AddScoped<IValidator<UpdatePostRequest>, UpdatePostRequestValidator>();
        services.AddScoped<IValidator<CommentRequest>, CommentRequestValidator>();

        return services;
    }
}
