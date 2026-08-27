using backend.Modules.AiCoaching.Configuration;
using Microsoft.Extensions.Options;

namespace backend.Modules.AiCoaching.Services;

public sealed class DailyCoachBriefingScheduleHostedService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly OpenAiOptions _openAiOptions;
    private readonly AiCoachInteractionOptions _options;
    private readonly ILogger<DailyCoachBriefingScheduleHostedService> _logger;

    public DailyCoachBriefingScheduleHostedService(IServiceScopeFactory scopeFactory, IOptions<OpenAiOptions> openAiOptions,
        IOptions<AiCoachInteractionOptions> options, ILogger<DailyCoachBriefingScheduleHostedService> logger)
    {
        _scopeFactory = scopeFactory;
        _openAiOptions = openAiOptions.Value;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_openAiOptions.Enabled)
            return;

        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(_options.DailyBriefingSchedulePollSeconds));
        while (!stoppingToken.IsCancellationRequested)
        {
            await ScheduleSafelyAsync(stoppingToken);
            await timer.WaitForNextTickAsync(stoppingToken);
        }
    }

    private async Task ScheduleSafelyAsync(CancellationToken cancellationToken)
    {
        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var queue = scope.ServiceProvider.GetRequiredService<ICoachInteractionQueueService>();
            await queue.ScheduleDueDailyBriefingsAsync(DateTime.UtcNow, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Daily coach briefing scheduler failed and will retry on its next interval.");
        }
    }
}
