using backend.Data;
using backend.Modules.Nutrition.Contracts;
using Microsoft.EntityFrameworkCore;

namespace backend.Modules.Nutrition.Services;

public interface IRecentFoodService
{
    Task<IReadOnlyList<RecentFoodResponse>> GetAsync(Guid userId, RecentFoodsFilter filter,
        CancellationToken cancellationToken);
}

public class RecentFoodService : IRecentFoodService
{
    private readonly FitspireDbContext _context;

    public RecentFoodService(FitspireDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<RecentFoodResponse>> GetAsync(Guid userId, RecentFoodsFilter filter,
        CancellationToken cancellationToken)
    {
        var query = OrderedActiveItemsForUser(userId);
        var seen = new HashSet<string>(StringComparer.Ordinal);
        var results = new List<RecentFoodResponse>();
        const int batchSize = 200;

        for (var offset = 0; results.Count < filter.Limit; offset += batchSize)
        {
            var batch = await query.Skip(offset).Take(batchSize).ToListAsync(cancellationToken);
            if (batch.Count == 0)
                break;

            foreach (var item in batch)
            {
                if (seen.Add(item.SnapshotKey))
                    results.Add(ToResponse(item));
                if (results.Count == filter.Limit)
                    break;
            }

            if (batch.Count < batchSize)
                break;
        }

        return results;
    }

    private IQueryable<RecentFoodRow> OrderedActiveItemsForUser(Guid userId) => _context.MealItems.AsNoTracking().Where(item =>
            item.DeletedAt == null && item.Meal.UserId == userId && item.Meal.DeletedAt == null)
        .OrderByDescending(item => item.Meal.MealDate)
        .ThenByDescending(item => item.Meal.ConsumedAtLocalTime)
        .ThenByDescending(item => item.UpdatedAt ?? item.CreatedAt)
        .ThenByDescending(item => item.Id)
        .Select(item => new RecentFoodRow(item.SnapshotKey, item.Name, item.Quantity, item.QuantityUnit,
            item.CustomUnitName, item.CaloriesKcal, item.ProteinGrams, item.CarbsGrams, item.FatGrams,
            item.Meal.MealDate, item.Meal.ConsumedAtLocalTime));

    private static RecentFoodResponse ToResponse(RecentFoodRow item) => new(item.Name, item.Quantity, item.QuantityUnit,
        item.CustomUnitName, item.CaloriesKcal, item.ProteinGrams, item.CarbsGrams, item.FatGrams, item.MealDate,
        item.ConsumedAtLocalTime);

    private sealed record RecentFoodRow(string SnapshotKey, string Name, decimal Quantity,
        Domain.Enums.QuantityUnit QuantityUnit, string? CustomUnitName, decimal? CaloriesKcal,
        decimal? ProteinGrams, decimal? CarbsGrams, decimal? FatGrams, DateOnly MealDate,
        TimeOnly? ConsumedAtLocalTime);
}
