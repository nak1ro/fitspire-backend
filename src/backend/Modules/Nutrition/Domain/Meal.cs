using backend.Modules.User.Domain;

namespace backend.Modules.Nutrition.Domain;

public class Meal
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }

    public DateTime Date { get; set; }
    public float? TotalCalories { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public AppUser User { get; set; } = null!;
    public ICollection<MealItem> Items { get; set; } = new List<MealItem>();
}