using backend.Modules.Nutrition.Domain.Constants;
using backend.Modules.Nutrition.Domain.Enums;
using backend.Modules.Shared.Domain;

namespace backend.Modules.Nutrition.Domain;

// Global, non-user-scoped reference catalog of common staple foods (banana, egg, chicken breast, ...)
// so a first-time user isn't stuck hand-typing macros for everyday items.
public class CommonFood : Entity<Guid>
{
    public string Code { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public string Category { get; private set; } = null!;
    public decimal Quantity { get; private set; }
    public QuantityUnit QuantityUnit { get; private set; }
    public string? CustomUnitName { get; private set; }
    public decimal? CaloriesKcal { get; private set; }
    public decimal? ProteinGrams { get; private set; }
    public decimal? CarbsGrams { get; private set; }
    public decimal? FatGrams { get; private set; }
    public int DisplayOrder { get; private set; }
    public bool IsActive { get; private set; } = true;

    private CommonFood() { }

    public static CommonFood Create(Guid id, string code, string category, int displayOrder, MealItemDraft draft)
    {
        var food = new CommonFood { Id = id, Code = code, CreatedAt = DateTime.UtcNow };
        food.Apply(category, displayOrder, draft);
        return food;
    }

    public void Synchronize(string category, int displayOrder, MealItemDraft draft)
    {
        Apply(category, displayOrder, draft);
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Retire()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public MealItemDraft ToMealItemDraft() => new(Name, Quantity, QuantityUnit, CustomUnitName, CaloriesKcal,
        ProteinGrams, CarbsGrams, FatGrams, null);

    private void Apply(string category, int displayOrder, MealItemDraft draft)
    {
        NutritionEntryRules.ValidateSnapshot(draft.Name, draft.Quantity, draft.QuantityUnit, draft.CustomUnitName,
            draft.CaloriesKcal, draft.ProteinGrams, draft.CarbsGrams, draft.FatGrams);
        Name = NutritionEntryRules.NormalizeRequiredName(draft.Name, "Food name");
        Category = category;
        DisplayOrder = displayOrder;
        Quantity = draft.Quantity;
        QuantityUnit = draft.QuantityUnit;
        CustomUnitName = NutritionEntryRules.NormalizeCustomUnit(draft.QuantityUnit, draft.CustomUnitName);
        CaloriesKcal = draft.CaloriesKcal;
        ProteinGrams = draft.ProteinGrams;
        CarbsGrams = draft.CarbsGrams;
        FatGrams = draft.FatGrams;
    }
}
