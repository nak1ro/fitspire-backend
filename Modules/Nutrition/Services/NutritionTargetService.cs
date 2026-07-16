using AutoMapper;
using backend.Data;
using backend.Modules.Nutrition.Contracts;
using backend.Modules.Nutrition.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Data;

namespace backend.Modules.Nutrition.Services;

public interface INutritionTargetService
{
    Task<NutritionTargetResponse?> GetAsync(Guid userId, CancellationToken cancellationToken);
    Task UpsertAsync(Guid userId, NutritionTargetRequest request, CancellationToken cancellationToken);
    Task DeleteAsync(Guid userId, CancellationToken cancellationToken);
}

public class NutritionTargetService : INutritionTargetService
{
    private readonly FitspireDbContext _context;
    private readonly IMapper _mapper;

    public NutritionTargetService(FitspireDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<NutritionTargetResponse?> GetAsync(Guid userId, CancellationToken cancellationToken)
    {
        var target = await _context.NutritionTargets.AsNoTracking()
            .FirstOrDefaultAsync(candidate => candidate.UserId == userId, cancellationToken);
        return target is null ? null : _mapper.Map<NutritionTargetResponse>(target);
    }

    public async Task UpsertAsync(Guid userId, NutritionTargetRequest request, CancellationToken cancellationToken)
    {
        await using var transaction = await BeginTransactionIfNeededAsync(cancellationToken);
        var target = await _context.NutritionTargets.FirstOrDefaultAsync(candidate => candidate.UserId == userId, cancellationToken);
        if (target is null)
            _context.NutritionTargets.Add(NutritionTarget.Create(Guid.NewGuid(), userId, request.CaloriesKcal,
                request.ProteinGrams, request.CarbsGrams, request.FatGrams));
        else
            target.Update(request.CaloriesKcal, request.ProteinGrams, request.CarbsGrams, request.FatGrams);

        await _context.SaveChangesAsync(cancellationToken);
        if (transaction is not null)
            await transaction.CommitAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid userId, CancellationToken cancellationToken)
    {
        await using var transaction = await BeginTransactionIfNeededAsync(cancellationToken);
        var target = await _context.NutritionTargets.FirstOrDefaultAsync(candidate => candidate.UserId == userId, cancellationToken);
        if (target is not null)
            _context.NutritionTargets.Remove(target);

        await _context.SaveChangesAsync(cancellationToken);
        if (transaction is not null)
            await transaction.CommitAsync(cancellationToken);
    }

    private async Task<IDbContextTransaction?> BeginTransactionIfNeededAsync(CancellationToken cancellationToken) =>
        _context.Database.CurrentTransaction is null
            ? await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken)
            : null;
}
