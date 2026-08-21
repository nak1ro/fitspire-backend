using backend.Data;
using backend.Modules.Nutrition.Domain;
using Microsoft.EntityFrameworkCore;

namespace backend.Modules.Nutrition.Data;

public static class CommonFoodSeeder
{
    public static async Task SeedAsync(FitspireDbContext context, CancellationToken cancellationToken = default)
    {
        var seeds = CommonFoodCatalogue.Definitions;
        var codes = seeds.Select(seed => seed.Code).ToHashSet();
        var existing = await context.CommonFoods.ToDictionaryAsync(food => food.Code, cancellationToken);

        foreach (var seed in seeds)
            Synchronize(context, existing, seed);

        var retired = existing.Values.Where(food => !codes.Contains(food.Code) && food.IsActive);
        foreach (var food in retired)
            food.Retire();

        await context.SaveChangesAsync(cancellationToken);
    }

    private static void Synchronize(FitspireDbContext context, IReadOnlyDictionary<string, CommonFood> existing, CommonFoodSeed seed)
    {
        var draft = new MealItemDraft(seed.Name, seed.Quantity, seed.QuantityUnit, seed.CustomUnitName,
            seed.CaloriesKcal, seed.ProteinGrams, seed.CarbsGrams, seed.FatGrams);

        if (existing.TryGetValue(seed.Code, out var food))
        {
            food.Synchronize(seed.Category, seed.DisplayOrder, draft);
            return;
        }

        context.CommonFoods.Add(CommonFood.Create(Guid.NewGuid(), seed.Code, seed.Category, seed.DisplayOrder, draft));
    }
}
