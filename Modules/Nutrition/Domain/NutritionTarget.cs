using backend.Modules.Nutrition.Domain.Constants;
using backend.Modules.Shared.Domain;
using backend.Modules.User.Domain;

namespace backend.Modules.Nutrition.Domain;

public class NutritionTarget : Entity<Guid>
{
    public Guid UserId { get; private set; }
    public decimal? CaloriesKcal { get; private set; }
    public decimal? ProteinGrams { get; private set; }
    public decimal? CarbsGrams { get; private set; }
    public decimal? FatGrams { get; private set; }
    public AppUser User { get; private set; } = null!;

    private NutritionTarget() { }

    public static NutritionTarget Create(Guid id, Guid userId, decimal? caloriesKcal, decimal? proteinGrams,
        decimal? carbsGrams, decimal? fatGrams)
    {
        if (id == Guid.Empty || userId == Guid.Empty)
            throw new DomainException("Nutrition target identity and owner are required.");

        var target = new NutritionTarget { Id = id, UserId = userId, CreatedAt = DateTime.UtcNow };
        target.Update(caloriesKcal, proteinGrams, carbsGrams, fatGrams);
        return target;
    }

    public void Update(decimal? caloriesKcal, decimal? proteinGrams, decimal? carbsGrams, decimal? fatGrams)
    {
        NutritionEntryRules.ValidateTarget(caloriesKcal, proteinGrams, carbsGrams, fatGrams);
        CaloriesKcal = caloriesKcal;
        ProteinGrams = proteinGrams;
        CarbsGrams = carbsGrams;
        FatGrams = fatGrams;
        UpdatedAt = DateTime.UtcNow;
    }
}
