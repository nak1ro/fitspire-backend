using backend.Modules.Auth.Authorization;
using backend.Modules.DemoData.Services;
using backend.Modules.Shared.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace backend.Modules.DemoData;

[ApiController]
[Route("api/admin/demo-data")]
[Authorize(Roles = AppRoles.Admin)]
public class DemoDataController : ControllerBase
{
    private readonly IDemoDataSeedProgress _progress;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<DemoDataController> _logger;

    public DemoDataController(IDemoDataSeedProgress progress, IServiceScopeFactory scopeFactory,
        ILogger<DemoDataController> logger)
    {
        _progress = progress;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    [HttpGet("status")]
    public IActionResult GetStatus() =>
        Ok(new { state = _progress.State.ToString(), errorMessage = _progress.ErrorMessage });

    // Runs the seeding work in the background and returns immediately — populating months of
    // workouts/meals/goals/badges/social activity through the real command layer takes long enough
    // that a synchronous request risks a gateway timeout. Poll GET status to see when it's done.
    [HttpPost("seed")]
    public IActionResult Seed()
    {
        if (_progress.State is DemoDataSeedState.Running or DemoDataSeedState.Completed)
            throw new ConflictException($"Demo data seeding is already {_progress.State.ToString().ToLowerInvariant()}.");

        _progress.MarkRunning();
        _ = RunInBackgroundAsync();
        return Accepted(new
        {
            message = "Demo data seeding started. This can take several minutes — poll GET /api/admin/demo-data/status."
        });
    }

    private async Task RunInBackgroundAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        var seeder = scope.ServiceProvider.GetRequiredService<IDemoDataSeedingService>();
        try
        {
            await seeder.SeedAsync(CancellationToken.None);
            _progress.MarkCompleted();
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Demo data seeding failed.");
            _progress.MarkFailed(exception.Message);
        }
    }
}
