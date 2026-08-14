using backend.Modules.AiCoaching.Configuration;
using Microsoft.Extensions.Options;

namespace backend.Modules.AiCoaching.Services;

public sealed class CoachInteractionGenerationHostedService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly OpenAiOptions _openAiOptions;
    private readonly AiCoachInteractionOptions _options;
    private readonly ILogger<CoachInteractionGenerationHostedService> _logger;

    public CoachInteractionGenerationHostedService(IServiceScopeFactory scopeFactory, IOptions<OpenAiOptions> openAiOptions,
        IOptions<AiCoachInteractionOptions> options, ILogger<CoachInteractionGenerationHostedService> logger)
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
            var processor = scope.ServiceProvider.GetRequiredService<ICoachInteractionGenerationService>();
            await processor.ProcessOneAsync(cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Coach interaction worker failed and will retry on its next interval.");
        }
    }
}
