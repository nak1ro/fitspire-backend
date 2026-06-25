using backend.Modules.Challenge.Services;
using backend.Modules.Shared;

namespace backend.Modules.Goal.Services;

public class GamificationLifecycleHostedService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    public GamificationLifecycleHostedService(IServiceScopeFactory scopeFactory) => _scopeFactory = scopeFactory;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(1));
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            using var scope = _scopeFactory.CreateScope();
            var goals = scope.ServiceProvider.GetRequiredService<IGoalProgressService>();
            var challenges = scope.ServiceProvider.GetRequiredService<IChallengeScoringService>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            await goals.ProcessDuePeriodsAsync(DateTime.UtcNow, stoppingToken);
            await challenges.ProcessLifecycleAsync(DateTime.UtcNow, stoppingToken);
            await unitOfWork.SaveChangesAsync(stoppingToken);
        }
    }
}
