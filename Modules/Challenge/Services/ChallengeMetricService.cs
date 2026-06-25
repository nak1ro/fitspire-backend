using backend.Data;
using backend.Modules.Progress.Services;
using backend.Modules.Shared.Domain;
using Microsoft.EntityFrameworkCore;

namespace backend.Modules.Challenge.Services;

public interface IChallengeMetricService
{
    Task EnsureSupportedAsync(string metricCode, string? workoutType, CancellationToken cancellationToken = default);
    Task<string> GetAggregationAsync(string metricCode, CancellationToken cancellationToken = default);
}

public class ChallengeMetricService : IChallengeMetricService
{
    private static readonly IReadOnlyDictionary<string, string> RequiredWorkoutTypes = new Dictionary<string, string>
    {
        [MetricCatalogue.RunningDistanceKm] = "running",
        [MetricCatalogue.CyclingDistanceKm] = "cycling",
        [MetricCatalogue.SwimmingDistanceMeters] = "swimming",
        [MetricCatalogue.YogaDurationMinutes] = "yoga",
        [MetricCatalogue.GymVolumeKg] = "gym",
        [MetricCatalogue.GymExerciseCount] = "gym"
    };

    private readonly FitspireDbContext _context;

    public ChallengeMetricService(FitspireDbContext context) => _context = context;

    public async Task EnsureSupportedAsync(string metricCode, string? workoutType, CancellationToken cancellationToken = default)
    {
        var metric = await _context.MetricDefinitions.FirstOrDefaultAsync(item => item.Id == metricCode && item.IsActive && item.IsChallengeSupported, cancellationToken)
            ?? throw new DomainException("Challenge metric is not supported.");

        if (RequiredWorkoutTypes.TryGetValue(metric.Id, out var requiredWorkoutType) &&
            !string.IsNullOrWhiteSpace(workoutType) && !string.Equals(workoutType, requiredWorkoutType, StringComparison.OrdinalIgnoreCase))
            throw new DomainException($"{metric.DisplayName} challenges can only use {requiredWorkoutType} workouts.");

        if (!string.IsNullOrWhiteSpace(workoutType) && workoutType.Trim().ToLowerInvariant() is not ("gym" or "running" or "cycling" or "swimming" or "yoga"))
            throw new DomainException("Challenge workout type is not supported.");
    }

    public async Task<string> GetAggregationAsync(string metricCode, CancellationToken cancellationToken = default) =>
        await _context.MetricDefinitions.Where(item => item.Id == metricCode).Select(item => item.Aggregation).FirstOrDefaultAsync(cancellationToken)
        ?? throw new DomainException("Challenge metric is not supported.");
}
