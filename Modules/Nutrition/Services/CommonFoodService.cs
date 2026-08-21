using AutoMapper;
using backend.Data;
using backend.Modules.Nutrition.Contracts;
using Microsoft.EntityFrameworkCore;

namespace backend.Modules.Nutrition.Services;

public interface ICommonFoodService
{
    Task<IReadOnlyList<CommonFoodResponse>> GetAsync(CommonFoodFilter filter, CancellationToken cancellationToken);
}

public class CommonFoodService : ICommonFoodService
{
    private readonly FitspireDbContext _context;
    private readonly IMapper _mapper;

    public CommonFoodService(FitspireDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<CommonFoodResponse>> GetAsync(CommonFoodFilter filter, CancellationToken cancellationToken)
    {
        var query = _context.CommonFoods.AsNoTracking().Where(food => food.IsActive);
        if (!string.IsNullOrWhiteSpace(filter.Query))
        {
            var term = filter.Query.Trim();
            query = query.Where(food => EF.Functions.ILike(food.Name, $"%{term}%"));
        }
        if (!string.IsNullOrWhiteSpace(filter.Category))
            query = query.Where(food => food.Category == filter.Category);

        var foods = await query.OrderBy(food => food.Category).ThenBy(food => food.DisplayOrder)
            .ToListAsync(cancellationToken);
        return _mapper.Map<IReadOnlyList<CommonFoodResponse>>(foods);
    }
}
