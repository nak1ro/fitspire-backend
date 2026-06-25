using backend.Modules.Challenge.Contracts;
using backend.Modules.Challenge.Services;
using FluentValidation;

namespace backend.Modules.Challenge;

public static class ChallengeModuleExtensions
{
    public static IServiceCollection AddChallengeModule(this IServiceCollection services)
    {
        services.AddScoped<IChallengeScoringService, ChallengeScoringService>();
        services.AddScoped<IChallengeTransactionService, ChallengeTransactionService>();
        services.AddScoped<IChallengeMetricService, ChallengeMetricService>();
        services.AddScoped<IChallengeAccessService, ChallengeAccessService>();
        services.AddScoped<IValidator<CreateChallengeRequest>, CreateChallengeRequestValidator>();
        services.AddScoped<IValidator<UpdateChallengeRequest>, UpdateChallengeRequestValidator>();
        services.AddScoped<IValidator<UpdateActiveChallengeCopyRequest>, UpdateActiveChallengeCopyRequestValidator>();
        services.AddScoped<IValidator<InviteChallengeUserRequest>, InviteChallengeUserRequestValidator>();
        services.AddScoped<IValidator<ChallengeListFilter>, ChallengeListFilterValidator>();
        return services;
    }
}
