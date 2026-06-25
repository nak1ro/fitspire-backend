using backend.Modules.Challenge.Services;
using backend.Modules.Shared;

namespace backend.Modules.Goal.Services;

public class GamificationLifecycleHostedService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<GamificationLifecycleHostedService> _logger;
    public GamificationLifecycleHostedService(IServiceScopeFactory scopeFactory, ILogger<GamificationLifecycleHostedService> logger) { _scopeFactory = scopeFactory; _logger = logger; }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(1));
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var goals = scope.ServiceProvider.GetRequiredService<IGoalProgressService>();
                var challenges = scope.ServiceProvider.GetRequiredService<IChallengeScoringService>();
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                await goals.ProcessDuePeriodsAsync(DateTime.UtcNow, stoppingToken);
                await unitOfWork.SaveChangesAsync(stoppingToken);
                await challenges.ProcessLifecycleAsync(DateTime.UtcNow, stoppingToken);
            }
            catch (Exception exception) when (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogError(exception, "Gamification lifecycle processing failed and will retry on the next interval.");
            }
        }
    }
}
