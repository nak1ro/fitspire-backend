using backend.Modules.Nutrition.Domain.Constants;
using backend.Modules.Nutrition.Domain.Enums;
using backend.Modules.Shared.Domain;

namespace backend.Modules.Nutrition.Domain;

public class MealItem : Entity<Guid>
{
    public Guid MealId { get; private set; }
    public string Name { get; private set; } = null!;
    public decimal Quantity { get; private set; }
    public QuantityUnit QuantityUnit { get; private set; }
    public string? CustomUnitName { get; private set; }
    public decimal? CaloriesKcal { get; private set; }
    public decimal? ProteinGrams { get; private set; }
    public decimal? CarbsGrams { get; private set; }
    public decimal? FatGrams { get; private set; }
    public int OrderIndex { get; private set; }
    public string SnapshotKey { get; private set; } = null!;
    public Guid? FavouriteFoodId { get; private set; }
    public DateTime? DeletedAt { get; private set; }
    public Meal Meal { get; private set; } = null!;

    private MealItem() { }

    internal static MealItem Create(Guid id, Guid mealId, int orderIndex, MealItemDraft draft)
    {
        if (id == Guid.Empty || mealId == Guid.Empty)
            throw new DomainException("Meal item identity and meal are required.");
        if (orderIndex < 1)
            throw new DomainException("Meal item order must be positive.");

        var item = new MealItem { Id = id, MealId = mealId, OrderIndex = orderIndex, CreatedAt = DateTime.UtcNow };
        item.Apply(draft, preserveFavouriteReference: false);
        return item;
    }

    internal void Update(MealItemDraft draft)
    {
        EnsureActive();
        Apply(draft, preserveFavouriteReference: true);
        UpdatedAt = DateTime.UtcNow;
    }

    internal void SetOrder(int orderIndex)
    {
        EnsureActive();
        if (orderIndex < 1)
            throw new DomainException("Meal item order must be positive.");
        OrderIndex = orderIndex;
        UpdatedAt = DateTime.UtcNow;
    }

    internal void SetTemporaryOrder(int orderIndex)
    {
        EnsureActive();
        if (orderIndex >= 0)
            throw new DomainException("Temporary meal item order must be negative.");
        OrderIndex = orderIndex;
        UpdatedAt = DateTime.UtcNow;
    }

    internal void SoftDelete()
    {
        EnsureActive();
        DeletedAt = DateTime.UtcNow;
        UpdatedAt = DeletedAt;
    }

    private void Apply(MealItemDraft draft, bool preserveFavouriteReference)
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
        SnapshotKey = NutritionEntryRules.CreateDefinitionKey(Name, Quantity, QuantityUnit, CustomUnitName,
            CaloriesKcal, ProteinGrams, CarbsGrams, FatGrams);
        if (!preserveFavouriteReference || draft.FavouriteFoodId.HasValue)
            FavouriteFoodId = draft.FavouriteFoodId;
    }

    private void EnsureActive()
    {
        if (DeletedAt is not null)
            throw new DomainException("A deleted meal item cannot be changed.");
    }

    public bool IsDeleted => DeletedAt is not null;
}
