using backend.Modules.Badge.Contracts;
using backend.Modules.Badge.Services;
using FluentValidation;

namespace backend.Modules.Badge;

public static class BadgeModuleExtensions
{
    public static IServiceCollection AddBadgeModule(this IServiceCollection services)
    {
        services.AddScoped<IBadgeEvaluationService, BadgeEvaluationService>();
        services.AddScoped<IBadgeAchievementSnapshotService, BadgeAchievementSnapshotService>();
        services.AddScoped<IBadgeTransactionService, BadgeTransactionService>();
        services.AddScoped<IBadgeUserLockService, BadgeUserLockService>();
        services.AddScoped<IValidator<BadgeCatalogueFilter>, BadgeCatalogueFilterValidator>();
        services.AddScoped<IValidator<BadgeCollectionFilter>, BadgeCollectionFilterValidator>();
        services.AddScoped<IValidator<PublicBadgeFilter>, PublicBadgeFilterValidator>();
        services.AddScoped<IValidator<SetFeaturedBadgesRequest>, SetFeaturedBadgesRequestValidator>();
        return services;
    }
}
