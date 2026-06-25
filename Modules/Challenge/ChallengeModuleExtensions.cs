using backend.Modules.Challenge.Contracts;
using backend.Modules.Challenge.Services;
using FluentValidation;

namespace backend.Modules.Challenge;

public static class ChallengeModuleExtensions
{
    public static IServiceCollection AddChallengeModule(this IServiceCollection services)
    {
        services.AddScoped<IChallengeScoringService, ChallengeScoringService>();
        services.AddScoped<IValidator<CreateChallengeRequest>, CreateChallengeRequestValidator>();
        services.AddScoped<IValidator<InviteChallengeUserRequest>, InviteChallengeUserRequestValidator>();
        return services;
    }
}
