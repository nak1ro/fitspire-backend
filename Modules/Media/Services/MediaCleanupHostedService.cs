using backend.Modules.Media.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace backend.Modules.Media.Services;

public class MediaCleanupHostedService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<MediaCleanupHostedService> _logger;
    private readonly TimeSpan _interval;

    public MediaCleanupHostedService(
        IServiceScopeFactory scopeFactory,
        IOptions<MediaStorageOptions> options,
        ILogger<MediaCleanupHostedService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _interval = TimeSpan.FromMinutes(options.Value.CleanupIntervalMinutes);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await RunCleanupAsync(stoppingToken);
            using var timer = new PeriodicTimer(_interval);

            while (await timer.WaitForNextTickAsync(stoppingToken))
                await RunCleanupAsync(stoppingToken);
        }
        catch (OperationCanceledException)
        {
            // Expected when the host is shutting down (e.g. dotnet watch restart).
        }
    }

    private async Task RunCleanupAsync(CancellationToken cancellationToken)
    {
        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var cleanup = scope.ServiceProvider.GetRequiredService<MediaCleanupService>();
            await cleanup.CleanExpiredMediaAsync(cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "An unexpected media cleanup error occurred.");
        }
    }
}
