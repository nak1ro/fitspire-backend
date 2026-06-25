using backend.Data;
using backend.Modules.Progress.Domain;
using backend.Modules.Progress.Services;
using Microsoft.EntityFrameworkCore;

namespace backend.Modules.Progress.Data;

public static class MetricDefinitionSeeder
{
    public static async Task SeedAsync(FitspireDbContext context, CancellationToken cancellationToken = default)
    {
        for (var index = 0; index < MetricCatalogue.Definitions.Count; index++)
        {
            var definition = MetricCatalogue.Definitions[index];
            var metric = await context.Set<MetricDefinition>().FindAsync([definition.Code], cancellationToken);
            if (metric is null)
            {
                await context.Set<MetricDefinition>().AddAsync(new MetricDefinition(
                    definition.Code,
                    definition.Name,
                    definition.Unit,
                    definition.Aggregation,
                    index + 1,
                    definition.IsGoalSupported,
                    definition.IsChallengeSupported,
                    definition.IsBadgeSupported,
                    definition.IsAnalyticsSupported), cancellationToken);
                continue;
            }

            metric.Synchronize(
                definition.Name,
                definition.Unit,
                definition.Aggregation,
                index + 1,
                definition.IsGoalSupported,
                definition.IsChallengeSupported,
                definition.IsBadgeSupported,
                definition.IsAnalyticsSupported);
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}
