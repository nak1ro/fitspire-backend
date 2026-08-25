using backend.Data;
using backend.Modules.Nutrition.Contracts;
using backend.Modules.Nutrition.Domain.Enums;
using backend.Modules.Nutrition.Features;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace backend.Modules.DemoData.Services;

public interface IDemoNutritionService
{
    Task SeedAsync(Guid userId, DateTime nowUtc, Random random, CancellationToken cancellationToken);
}

public class DemoNutritionService : IDemoNutritionService
{
    private readonly IMediator _mediator;
    private readonly FitspireDbContext _context;

    public DemoNutritionService(IMediator mediator, FitspireDbContext context)
    {
        _mediator = mediator;
        _context = context;
    }

    public async Task SeedAsync(Guid userId, DateTime nowUtc, Random random, CancellationToken cancellationToken)
    {
        await _mediator.Send(new UpsertNutritionTargetCommand(userId,
            new NutritionTargetRequest(2400, 150, 250, 80)), cancellationToken);

        var foods = await _context.CommonFoods.AsNoTracking().Where(food => food.IsActive).ToListAsync(cancellationToken);
        if (foods.Count == 0) return;

        var mealTypes = new[] { MealType.Breakfast, MealType.Lunch, MealType.Dinner, MealType.Snack };
        for (var dayOffset = 55; dayOffset >= 0; dayOffset--)
        {
            if (random.NextDouble() > 0.7) continue;
            var date = DateOnly.FromDateTime(nowUtc.AddDays(-dayOffset));
            var mealsToday = random.Next(2, 4);
            for (var i = 0; i < mealsToday; i++)
            {
                var items = Enumerable.Range(0, random.Next(1, 4))
                    .Select(_ => foods[random.Next(foods.Count)])
                    .Select(food => new MealItemRequest(food.Name, food.Quantity, food.QuantityUnit, food.CustomUnitName,
                        food.CaloriesKcal, food.ProteinGrams, food.CarbsGrams, food.FatGrams))
                    .ToList();
                await _mediator.Send(new CreateMealCommand(userId,
                    new CreateMealRequest(date, null, mealTypes[i % mealTypes.Length], null, null, items)), cancellationToken);
            }
        }
    }
}
