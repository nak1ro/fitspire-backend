using backend.Modules.Social.Infrastructure;
using backend.Modules.Social.Validators;
using backend.Modules.Social.Contracts.Posts;
using backend.Modules.Social.Contracts.Comments;
using backend.Modules.Social.Services;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace backend.Modules.Social;

public static class SocialModuleExtensions
{
    public static IServiceCollection AddSocialModule(this IServiceCollection services)
    {
        services.AddScoped<ISocialRepository, SocialRepository>();
        services.AddScoped<ISocialAccessService, SocialAccessService>();
        services.AddScoped<IValidator<CreatePostRequest>, CreatePostRequestValidator>();
        services.AddScoped<IValidator<UpdatePostRequest>, UpdatePostRequestValidator>();
        services.AddScoped<IValidator<ShareWorkoutRequest>, ShareWorkoutRequestValidator>();
        services.AddScoped<IValidator<CommentRequest>, CommentRequestValidator>();
        services.AddScoped<IValidator<UpdateCommentRequest>, UpdateCommentRequestValidator>();

        return services;
    }
}
