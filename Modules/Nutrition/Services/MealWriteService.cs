using backend.Data;
using backend.Modules.Nutrition.Contracts;
using backend.Modules.Nutrition.Domain;
using backend.Modules.Shared.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Data;

namespace backend.Modules.Nutrition.Services;

public interface IMealWriteService
{
    Task<Guid> CreateAsync(Guid userId, CreateMealRequest request, CancellationToken cancellationToken);
    Task UpdateAsync(Guid userId, Guid mealId, UpdateMealRequest request, CancellationToken cancellationToken);
    Task SoftDeleteAsync(Guid userId, Guid mealId, CancellationToken cancellationToken);
    Task<Guid> AddItemAsync(Guid userId, Guid mealId, AddMealItemRequest request, CancellationToken cancellationToken);
    Task UpdateItemAsync(Guid userId, Guid mealId, Guid itemId, MealItemRequest request, CancellationToken cancellationToken);
    Task RemoveItemAsync(Guid userId, Guid mealId, Guid itemId, CancellationToken cancellationToken);
    Task ReorderItemsAsync(Guid userId, Guid mealId, ReorderMealItemsRequest request, CancellationToken cancellationToken);
}

public class MealWriteService : IMealWriteService
{
    private readonly FitspireDbContext _context;
    private readonly INutritionTimeZoneService _timeZoneService;

    public MealWriteService(FitspireDbContext context, INutritionTimeZoneService timeZoneService)
    {
        _context = context;
        _timeZoneService = timeZoneService;
    }

    public async Task<Guid> CreateAsync(Guid userId, CreateMealRequest request, CancellationToken cancellationToken)
    {
        await EnsureDateIsNotFutureAsync(userId, request.MealDate, cancellationToken);
        await using var transaction = await BeginTransactionIfNeededAsync(cancellationToken);
        var meal = Meal.Create(Guid.NewGuid(), userId, request.MealDate, request.ConsumedAtLocalTime, request.MealType,
            request.Name, request.Notes, request.Items.Select(ToDraft).ToList());

        _context.Meals.Add(meal);
        await _context.SaveChangesAsync(cancellationToken);
        await CommitAsync(transaction, cancellationToken);
        return meal.Id;
    }

    public async Task UpdateAsync(Guid userId, Guid mealId, UpdateMealRequest request, CancellationToken cancellationToken)
    {
        await EnsureDateIsNotFutureAsync(userId, request.MealDate, cancellationToken);
        await using var transaction = await BeginTransactionIfNeededAsync(cancellationToken);
        var meal = await LoadOwnedActiveMealAsync(userId, mealId, cancellationToken);
        meal.ChangeDate(request.MealDate);
        meal.UpdateDetails(request.ConsumedAtLocalTime, request.MealType, request.Name, request.Notes);
        await SaveAndCommitAsync(transaction, cancellationToken);
    }

    public async Task SoftDeleteAsync(Guid userId, Guid mealId, CancellationToken cancellationToken)
    {
        await using var transaction = await BeginTransactionIfNeededAsync(cancellationToken);
        var meal = await LoadOwnedActiveMealAsync(userId, mealId, cancellationToken);
        meal.SoftDelete();
        await SaveAndCommitAsync(transaction, cancellationToken);
    }

    public async Task<Guid> AddItemAsync(Guid userId, Guid mealId, AddMealItemRequest request, CancellationToken cancellationToken)
    {
        await using var transaction = await BeginTransactionIfNeededAsync(cancellationToken);
        var meal = await LoadOwnedActiveMealAsync(userId, mealId, cancellationToken);
        var item = meal.AddItem(await ResolveItemDraftAsync(userId, request, cancellationToken));
        await SaveAndCommitAsync(transaction, cancellationToken);
        return item.Id;
    }

    public async Task UpdateItemAsync(Guid userId, Guid mealId, Guid itemId, MealItemRequest request,
        CancellationToken cancellationToken)
    {
        await using var transaction = await BeginTransactionIfNeededAsync(cancellationToken);
        var meal = await LoadOwnedActiveMealAsync(userId, mealId, cancellationToken);
        meal.UpdateItem(itemId, ToDraft(request));
        await SaveAndCommitAsync(transaction, cancellationToken);
    }

    public async Task RemoveItemAsync(Guid userId, Guid mealId, Guid itemId, CancellationToken cancellationToken)
    {
        await using var transaction = await BeginTransactionIfNeededAsync(cancellationToken);
        var meal = await LoadOwnedActiveMealAsync(userId, mealId, cancellationToken);
        await PrepareOrderingAsync(meal, cancellationToken);
        meal.RemoveItem(itemId);
        await SaveAndCommitAsync(transaction, cancellationToken);
    }

    public async Task ReorderItemsAsync(Guid userId, Guid mealId, ReorderMealItemsRequest request,
        CancellationToken cancellationToken)
    {
        await using var transaction = await BeginTransactionIfNeededAsync(cancellationToken);
        var meal = await LoadOwnedActiveMealAsync(userId, mealId, cancellationToken);
        await PrepareOrderingAsync(meal, cancellationToken);
        meal.ReorderItems(request.ItemIds);
        await SaveAndCommitAsync(transaction, cancellationToken);
    }

    private async Task<Meal> LoadOwnedActiveMealAsync(Guid userId, Guid mealId, CancellationToken cancellationToken) =>
        await _context.Meals.Include(meal => meal.Items).FirstOrDefaultAsync(meal =>
                meal.Id == mealId && meal.UserId == userId && meal.DeletedAt == null, cancellationToken)
            ?? throw new NotFoundException("Meal was not found.");

    private async Task PrepareOrderingAsync(Meal meal, CancellationToken cancellationToken)
    {
        meal.PrepareItemReorder();
        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task EnsureDateIsNotFutureAsync(Guid userId, DateOnly date, CancellationToken cancellationToken)
    {
        if (date > await _timeZoneService.GetTodayAsync(userId, cancellationToken))
            throw new DomainException("Meal date cannot be in the future in the user's timezone.");
    }

    private static MealItemDraft ToDraft(MealItemRequest request) => new(request.Name, request.Quantity,
        request.QuantityUnit, request.CustomUnitName, request.CaloriesKcal, request.ProteinGrams,
        request.CarbsGrams, request.FatGrams);

    private async Task<MealItemDraft> ResolveItemDraftAsync(Guid userId, AddMealItemRequest request,
        CancellationToken cancellationToken)
    {
        if (request.Item is not null)
            return ToDraft(request.Item);

        var favourite = await _context.FavouriteFoods.FirstOrDefaultAsync(food =>
                food.Id == request.FavouriteFoodId && food.UserId == userId && food.DeletedAt == null, cancellationToken)
            ?? throw new NotFoundException("Favourite food was not found.");
        return favourite.ToMealItemDraft();
    }

    private async Task<IDbContextTransaction?> BeginTransactionIfNeededAsync(CancellationToken cancellationToken) =>
        _context.Database.CurrentTransaction is null
            ? await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken)
            : null;

    private async Task SaveAndCommitAsync(IDbContextTransaction? transaction, CancellationToken cancellationToken)
    {
        await _context.SaveChangesAsync(cancellationToken);
        await CommitAsync(transaction, cancellationToken);
    }

    private static Task CommitAsync(IDbContextTransaction? transaction, CancellationToken cancellationToken) =>
        transaction is null ? Task.CompletedTask : transaction.CommitAsync(cancellationToken);
}
