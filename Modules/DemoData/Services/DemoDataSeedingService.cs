using backend.Modules.Goal.Services;
using Microsoft.Extensions.Logging;

namespace backend.Modules.DemoData.Services;

public interface IDemoDataSeedingService
{
    Task SeedAsync(CancellationToken cancellationToken);
}

public class DemoDataSeedingService : IDemoDataSeedingService
{
    private readonly IDemoAccountService _accounts;
    private readonly IDemoWorkoutService _workouts;
    private readonly IDemoNutritionService _nutrition;
    private readonly IDemoGoalService _goals;
    private readonly IDemoChallengeService _challenges;
    private readonly IDemoSocialService _social;
    private readonly IGoalProgressService _goalProgress;
    private readonly ILogger<DemoDataSeedingService> _logger;

    public DemoDataSeedingService(IDemoAccountService accounts, IDemoWorkoutService workouts,
        IDemoNutritionService nutrition, IDemoGoalService goals, IDemoChallengeService challenges,
        IDemoSocialService social, IGoalProgressService goalProgress, ILogger<DemoDataSeedingService> logger)
    {
        _accounts = accounts;
        _workouts = workouts;
        _nutrition = nutrition;
        _goals = goals;
        _challenges = challenges;
        _social = social;
        _goalProgress = goalProgress;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        var random = new Random(42);
        var nowUtc = DateTime.UtcNow;

        _logger.LogInformation("Demo data seeding: creating accounts.");
        var heroId = await _accounts.CreateHeroAsync(cancellationToken);
        var fillerIds = await _accounts.CreateFillersAsync(cancellationToken);

        // Goals can't be backdated, so they only see activity logged after they exist — create them
        // first, then log a small burst of genuinely fresh (today, post-creation) workouts to give
        // them real progress, on top of the separate historical backfill used purely for badges.
        _logger.LogInformation("Demo data seeding: hero goals.");
        await _goals.SeedAsync(heroId, nowUtc, cancellationToken);

        _logger.LogInformation("Demo data seeding: hero fresh goal-qualifying activity.");
        await _workouts.SeedFreshGoalActivityAsync(heroId, random, cancellationToken);

        _logger.LogInformation("Demo data seeding: hero workout history.");
        await _workouts.SeedRichHistoryAsync(heroId, nowUtc, random, cancellationToken);

        _logger.LogInformation("Demo data seeding: filler workout history.");
        foreach (var fillerId in fillerIds)
            await _workouts.SeedLightHistoryAsync(fillerId, nowUtc, random, cancellationToken);

        _logger.LogInformation("Demo data seeding: nutrition.");
        await _nutrition.SeedAsync(heroId, nowUtc, random, cancellationToken);

        // Closes any due recurring goal periods and evaluates the badges tied to goal completion —
        // normally handled by a background job that isn't running during this one-off script.
        _logger.LogInformation("Demo data seeding: closing recurring goal periods.");
        await _goalProgress.ProcessDuePeriodsAsync(nowUtc, cancellationToken);

        _logger.LogInformation("Demo data seeding: challenge.");
        await _challenges.SeedAsync(heroId, fillerIds, random, cancellationToken);

        _logger.LogInformation("Demo data seeding: social graph and feed.");
        await _social.SeedAsync(heroId, fillerIds, random, cancellationToken);

        _logger.LogInformation("Demo data seeding complete.");
    }
}
