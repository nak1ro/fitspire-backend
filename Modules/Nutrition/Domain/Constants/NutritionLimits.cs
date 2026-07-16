namespace backend.Modules.Nutrition.Domain.Constants;

public static class NutritionLimits
{
    public const int MaximumFoodNameLength = 200;
    public const int MaximumCustomUnitNameLength = 50;
    public const int MaximumMealNameLength = 100;
    public const int MaximumMealNotesLength = 1_000;
    public const int MaximumItemsPerMeal = 100;
    public const decimal MaximumQuantity = 1_000_000m;
    public const decimal MaximumNutrientValue = 1_000_000m;
    public const decimal MaximumDailyTarget = 1_000_000m;
}
