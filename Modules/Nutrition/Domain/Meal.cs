using backend.Modules.Nutrition.Domain.Constants;
using backend.Modules.Nutrition.Domain.Enums;
using backend.Modules.Shared.Domain;
using backend.Modules.User.Domain;

namespace backend.Modules.Nutrition.Domain;

public class Meal : AggregateRoot<Guid>
{
    private readonly List<MealItem> _items = [];

    public Guid UserId { get; private set; }
    public DateOnly MealDate { get; private set; }
    public TimeOnly? ConsumedAtLocalTime { get; private set; }
    public MealType MealType { get; private set; }
    public string? Name { get; private set; }
    public string? Notes { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    public AppUser User { get; private set; } = null!;
    public IReadOnlyCollection<MealItem> Items => _items.AsReadOnly();

    private Meal() { }

    public static Meal Create(Guid id, Guid userId, DateOnly mealDate, TimeOnly? consumedAtLocalTime,
        MealType mealType, string? name, string? notes, IReadOnlyCollection<MealItemDraft> items)
    {
        if (id == Guid.Empty || userId == Guid.Empty)
            throw new DomainException("Meal identity and owner are required.");
        if (mealDate == DateOnly.MinValue)
            throw new DomainException("Meal date is required.");
        if (items.Count == 0)
            throw new DomainException("A meal must contain at least one item.");
        if (items.Count > NutritionLimits.MaximumItemsPerMeal)
            throw new DomainException($"A meal can contain no more than {NutritionLimits.MaximumItemsPerMeal} items.");

        var meal = new Meal { Id = id, UserId = userId, MealDate = mealDate, CreatedAt = DateTime.UtcNow };
        meal.UpdateDetails(consumedAtLocalTime, mealType, name, notes);
        foreach (var item in items)
            meal.AddItem(item);
        return meal;
    }

    public void UpdateDetails(TimeOnly? consumedAtLocalTime, MealType mealType, string? name, string? notes)
    {
        EnsureActive();
        if (!Enum.IsDefined(mealType))
            throw new DomainException("Meal type is not supported.");
        ConsumedAtLocalTime = consumedAtLocalTime;
        MealType = mealType;
        Name = NutritionEntryRules.NormalizeOptionalText(name, NutritionLimits.MaximumMealNameLength, "Meal name");
        Notes = NutritionEntryRules.NormalizeOptionalText(notes, NutritionLimits.MaximumMealNotesLength, "Meal notes");
        UpdatedAt = DateTime.UtcNow;
    }

    public void ChangeDate(DateOnly mealDate)
    {
        EnsureActive();
        if (mealDate == DateOnly.MinValue)
            throw new DomainException("Meal date is required.");
        MealDate = mealDate;
        UpdatedAt = DateTime.UtcNow;
    }

    public MealItem AddItem(MealItemDraft draft)
    {
        EnsureActive();
        if (_items.Count(existing => !existing.IsDeleted) >= NutritionLimits.MaximumItemsPerMeal)
            throw new DomainException($"A meal can contain no more than {NutritionLimits.MaximumItemsPerMeal} items.");
        var item = MealItem.Create(Guid.NewGuid(), Id, _items.Count(existing => !existing.IsDeleted) + 1, draft);
        _items.Add(item);
        UpdatedAt = DateTime.UtcNow;
        return item;
    }

    public void UpdateItem(Guid itemId, MealItemDraft draft)
    {
        EnsureActive();
        FindActiveItem(itemId).Update(draft);
        UpdatedAt = DateTime.UtcNow;
    }

    public void RemoveItem(Guid itemId)
    {
        EnsureActive();
        if (_items.Count(item => !item.IsDeleted) <= 1)
            throw new DomainException("A meal must contain at least one item.");
        FindActiveItem(itemId).SoftDelete();
        NormalizeItemOrder();
        UpdatedAt = DateTime.UtcNow;
    }

    public void ReorderItems(IReadOnlyCollection<Guid> orderedItemIds)
    {
        EnsureActive();
        var activeItems = _items.Where(item => !item.IsDeleted).ToList();
        if (orderedItemIds.Count != activeItems.Count || orderedItemIds.Distinct().Count() != orderedItemIds.Count ||
            orderedItemIds.Any(itemId => activeItems.All(item => item.Id != itemId)))
            throw new DomainException("Item reorder must contain every active item exactly once.");

        foreach (var (itemId, index) in orderedItemIds.Select((itemId, index) => (itemId, index)))
            FindActiveItem(itemId).SetOrder(index + 1);
        UpdatedAt = DateTime.UtcNow;
    }

    public void PrepareItemReorder()
    {
        EnsureActive();
        foreach (var (item, index) in _items.Where(item => !item.IsDeleted).Select((item, index) => (item, index)))
            item.SetTemporaryOrder(-(index + 1));
        UpdatedAt = DateTime.UtcNow;
    }

    public NutritionTotals CalculateTotals()
    {
        var activeItems = _items.Where(item => !item.IsDeleted);
        return new NutritionTotals(activeItems.Sum(item => item.CaloriesKcal ?? 0),
            activeItems.Sum(item => item.ProteinGrams ?? 0), activeItems.Sum(item => item.CarbsGrams ?? 0),
            activeItems.Sum(item => item.FatGrams ?? 0));
    }

    public void SoftDelete()
    {
        EnsureActive();
        DeletedAt = DateTime.UtcNow;
        UpdatedAt = DeletedAt;
    }

    private MealItem FindActiveItem(Guid itemId) => _items.FirstOrDefault(item => item.Id == itemId && !item.IsDeleted)
        ?? throw new DomainException("Meal item was not found.");

    private void NormalizeItemOrder()
    {
        foreach (var (item, index) in _items.Where(item => !item.IsDeleted).OrderBy(item => item.OrderIndex)
                     .Select((item, index) => (item, index)))
            item.SetOrder(index + 1);
    }

    private void EnsureActive()
    {
        if (DeletedAt is not null)
            throw new DomainException("A deleted meal cannot be changed.");
    }
}
