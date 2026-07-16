using AutoMapper;
using backend.Data;
using backend.Modules.Nutrition.Contracts;
using backend.Modules.Nutrition.Domain;
using backend.Modules.Shared.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Data;

namespace backend.Modules.Nutrition.Services;

public interface IFavouriteFoodService
{
    Task<Guid> CreateAsync(Guid userId, FavouriteFoodRequest request, CancellationToken cancellationToken);
    Task UpdateAsync(Guid userId, Guid favouriteId, FavouriteFoodRequest request, CancellationToken cancellationToken);
    Task SoftDeleteAsync(Guid userId, Guid favouriteId, CancellationToken cancellationToken);
    Task<FavouriteFoodPageResponse> GetPageAsync(Guid userId, FavouriteFoodFilter filter, CancellationToken cancellationToken);
}

public class FavouriteFoodService : IFavouriteFoodService
{
    private readonly FitspireDbContext _context;
    private readonly IMapper _mapper;

    public FavouriteFoodService(FitspireDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Guid> CreateAsync(Guid userId, FavouriteFoodRequest request, CancellationToken cancellationToken)
    {
        await using var transaction = await BeginTransactionIfNeededAsync(cancellationToken);
        var favourite = FavouriteFood.Create(Guid.NewGuid(), userId, ToDraft(request));
        _context.FavouriteFoods.Add(favourite);
        await SaveAndCommitAsync(transaction, cancellationToken);
        return favourite.Id;
    }

    public async Task UpdateAsync(Guid userId, Guid favouriteId, FavouriteFoodRequest request,
        CancellationToken cancellationToken)
    {
        await using var transaction = await BeginTransactionIfNeededAsync(cancellationToken);
        var favourite = await LoadOwnedActiveFavouriteAsync(userId, favouriteId, cancellationToken);
        favourite.Update(ToDraft(request));
        await SaveAndCommitAsync(transaction, cancellationToken);
    }

    public async Task SoftDeleteAsync(Guid userId, Guid favouriteId, CancellationToken cancellationToken)
    {
        await using var transaction = await BeginTransactionIfNeededAsync(cancellationToken);
        var favourite = await LoadOwnedActiveFavouriteAsync(userId, favouriteId, cancellationToken);
        favourite.SoftDelete();
        await SaveAndCommitAsync(transaction, cancellationToken);
    }

    public async Task<FavouriteFoodPageResponse> GetPageAsync(Guid userId, FavouriteFoodFilter filter,
        CancellationToken cancellationToken)
    {
        var query = _context.FavouriteFoods.AsNoTracking()
            .Where(food => food.UserId == userId && food.DeletedAt == null);
        if (!string.IsNullOrWhiteSpace(filter.Query))
        {
            var term = filter.Query.Trim();
            query = query.Where(food => EF.Functions.ILike(food.Name, $"%{term}%"));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var foods = await query.OrderBy(food => food.Name).ThenBy(food => food.CreatedAt)
            .Skip((filter.Page - 1) * filter.PageSize).Take(filter.PageSize).ToListAsync(cancellationToken);
        return new FavouriteFoodPageResponse(_mapper.Map<IReadOnlyList<FavouriteFoodResponse>>(foods), filter.Page,
            filter.PageSize, totalCount);
    }

    private async Task<FavouriteFood> LoadOwnedActiveFavouriteAsync(Guid userId, Guid favouriteId,
        CancellationToken cancellationToken) =>
        await _context.FavouriteFoods.FirstOrDefaultAsync(food =>
                food.Id == favouriteId && food.UserId == userId && food.DeletedAt == null, cancellationToken)
            ?? throw new NotFoundException("Favourite food was not found.");

    private static MealItemDraft ToDraft(FavouriteFoodRequest request) => new(request.Name, request.Quantity,
        request.QuantityUnit, request.CustomUnitName, request.CaloriesKcal, request.ProteinGrams,
        request.CarbsGrams, request.FatGrams);

    private async Task SaveAndCommitAsync(IDbContextTransaction? transaction, CancellationToken cancellationToken)
    {
        await _context.SaveChangesAsync(cancellationToken);
        if (transaction is not null)
            await transaction.CommitAsync(cancellationToken);
    }

    private async Task<IDbContextTransaction?> BeginTransactionIfNeededAsync(CancellationToken cancellationToken) =>
        _context.Database.CurrentTransaction is null
            ? await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken)
            : null;
}
