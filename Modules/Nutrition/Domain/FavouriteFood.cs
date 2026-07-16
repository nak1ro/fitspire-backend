using backend.Modules.Nutrition.Domain.Constants;
using backend.Modules.Nutrition.Domain.Enums;
using backend.Modules.Shared.Domain;
using backend.Modules.User.Domain;

namespace backend.Modules.Nutrition.Domain;

public class FavouriteFood : Entity<Guid>
{
    public Guid UserId { get; private set; }
    public string Name { get; private set; } = null!;
    public string DefinitionKey { get; private set; } = null!;
    public decimal Quantity { get; private set; }
    public QuantityUnit QuantityUnit { get; private set; }
    public string? CustomUnitName { get; private set; }
    public decimal? CaloriesKcal { get; private set; }
    public decimal? ProteinGrams { get; private set; }
    public decimal? CarbsGrams { get; private set; }
    public decimal? FatGrams { get; private set; }
    public DateTime? DeletedAt { get; private set; }
    public AppUser User { get; private set; } = null!;

    private FavouriteFood() { }

    public static FavouriteFood Create(Guid id, Guid userId, MealItemDraft draft)
    {
        if (id == Guid.Empty || userId == Guid.Empty)
            throw new DomainException("Favourite food identity and owner are required.");

        var favourite = new FavouriteFood { Id = id, UserId = userId, CreatedAt = DateTime.UtcNow };
        favourite.Apply(draft);
        return favourite;
    }

    public void Update(MealItemDraft draft)
    {
        EnsureActive();
        Apply(draft);
        UpdatedAt = DateTime.UtcNow;
    }

    public void SoftDelete()
    {
        EnsureActive();
        DeletedAt = DateTime.UtcNow;
        UpdatedAt = DeletedAt;
    }

    public MealItemDraft ToMealItemDraft() => new(Name, Quantity, QuantityUnit, CustomUnitName, CaloriesKcal,
        ProteinGrams, CarbsGrams, FatGrams, Id);

    private void Apply(MealItemDraft draft)
    {
        NutritionEntryRules.ValidateSnapshot(draft.Name, draft.Quantity, draft.QuantityUnit, draft.CustomUnitName,
            draft.CaloriesKcal, draft.ProteinGrams, draft.CarbsGrams, draft.FatGrams);
        Name = NutritionEntryRules.NormalizeRequiredName(draft.Name, "Food name");
        Quantity = draft.Quantity;
        QuantityUnit = draft.QuantityUnit;
        CustomUnitName = NutritionEntryRules.NormalizeCustomUnit(draft.QuantityUnit, draft.CustomUnitName);
        CaloriesKcal = draft.CaloriesKcal;
        ProteinGrams = draft.ProteinGrams;
        CarbsGrams = draft.CarbsGrams;
        FatGrams = draft.FatGrams;
        DefinitionKey = NutritionEntryRules.CreateDefinitionKey(Name, Quantity, QuantityUnit, CustomUnitName,
            CaloriesKcal, ProteinGrams, CarbsGrams, FatGrams);
    }

    private void EnsureActive()
    {
        if (DeletedAt is not null)
            throw new DomainException("A deleted favourite food cannot be changed.");
    }

}
