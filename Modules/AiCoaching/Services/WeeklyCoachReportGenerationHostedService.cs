using backend.Modules.AiCoaching.Configuration;
using Microsoft.Extensions.Options;

namespace backend.Modules.AiCoaching.Services;

public sealed class WeeklyCoachReportGenerationHostedService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly OpenAiOptions _options;
    private readonly ILogger<WeeklyCoachReportGenerationHostedService> _logger;

    public WeeklyCoachReportGenerationHostedService(IServiceScopeFactory scopeFactory, IOptions<OpenAiOptions> options,
        ILogger<WeeklyCoachReportGenerationHostedService> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Enabled)
            return;

        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(_options.WorkerPollSeconds));
        while (!stoppingToken.IsCancellationRequested)
        {
            await ProcessOneSafelyAsync(stoppingToken);
            await timer.WaitForNextTickAsync(stoppingToken);
        }
    }

    private async Task ProcessOneSafelyAsync(CancellationToken cancellationToken)
    {
        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var processor = scope.ServiceProvider.GetRequiredService<IWeeklyCoachReportGenerationService>();
            await processor.ProcessOneAsync(cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Weekly coach report worker failed and will retry on its next interval.");
        }
    }
}
