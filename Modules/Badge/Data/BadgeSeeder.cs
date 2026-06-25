using backend.Data;
using backend.Modules.Badge.Domain;
using backend.Modules.Badge.Domain.Constants;
using Microsoft.EntityFrameworkCore;

namespace backend.Modules.Badge.Data;

public static class BadgeSeeder
{
    public static async Task SeedAsync(FitspireDbContext context, CancellationToken cancellationToken = default)
    {
        var definitions = BadgeDefinitionCatalogue.Definitions;
        await ValidateDefinitionsAsync(context, definitions, cancellationToken);

        var existing = await context.Badges.ToDictionaryAsync(badge => badge.Code, StringComparer.OrdinalIgnoreCase, cancellationToken);
        foreach (var definition in definitions)
        {
            if (existing.TryGetValue(definition.Code, out var badge))
                badge.Synchronize(definition);
            else
                await context.Badges.AddAsync(AchievementBadge.Create(definition), cancellationToken);
        }

        foreach (var code in BadgeDefinitionCatalogue.RetiredCodes)
            if (existing.TryGetValue(code, out var badge))
                badge.Retire();

        await context.SaveChangesAsync(cancellationToken);
    }

    private static async Task ValidateDefinitionsAsync(FitspireDbContext context, IReadOnlyList<BadgeDefinition> definitions,
        CancellationToken cancellationToken)
    {
        if (definitions.Select(definition => definition.Code).Distinct(StringComparer.OrdinalIgnoreCase).Count() != definitions.Count)
            throw new InvalidOperationException("Badge definition codes must be unique.");
        if (definitions.Select(definition => definition.DisplayOrder).Distinct().Count() != definitions.Count)
            throw new InvalidOperationException("Badge definition display orders must be unique.");

        foreach (var definition in definitions)
        {
            definition.EnsureValid();
            if (!BadgeCriterionCodes.IsKnown(definition.CriterionCode))
                throw new InvalidOperationException($"Badge criterion '{definition.CriterionCode}' is not registered.");
        }

        var metricCodes = definitions.Where(definition => !string.IsNullOrWhiteSpace(definition.MetricCode))
            .Select(definition => definition.MetricCode!).Distinct(StringComparer.Ordinal).ToList();
        var supportedMetricCodes = await context.MetricDefinitions.Where(metric => metricCodes.Contains(metric.Id) && metric.IsBadgeSupported)
            .Select(metric => metric.Id).ToListAsync(cancellationToken);
        var missingMetric = metricCodes.Except(supportedMetricCodes, StringComparer.Ordinal).FirstOrDefault();
        if (missingMetric is not null)
            throw new InvalidOperationException($"Badge metric '{missingMetric}' is not active and badge-supported.");
    }
}
