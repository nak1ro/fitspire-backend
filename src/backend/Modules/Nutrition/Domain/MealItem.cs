namespace backend.Modules.Nutrition.Domain;

public class MealItem
{
    public Guid Id { get; set; }
    public Guid MealId { get; set; }

    public string Name { get; set; } = null!;
    public float? Calories { get; set; }
    public float? Protein { get; set; }
    public float? Carbs { get; set; }
    public float? Fat { get; set; }
    public float? Quantity { get; set; }
    public string? Unit { get; set; }

    // Navigation
    public Meal Meal { get; set; } = null!;
}