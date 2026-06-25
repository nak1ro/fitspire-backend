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
            var (code, name, unit, aggregation) = MetricCatalogue.Definitions[index];
            if (!await context.Set<MetricDefinition>().AnyAsync(metric => metric.Id == code, cancellationToken))
                await context.Set<MetricDefinition>().AddAsync(new MetricDefinition(code, name, unit, aggregation, index + 1), cancellationToken);
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}
