using AutoMapper;
using backend.Data;
using backend.Modules.Nutrition.Contracts;
using backend.Modules.Nutrition.Domain;
using backend.Modules.Shared.Domain;
using Microsoft.EntityFrameworkCore;

namespace backend.Modules.Nutrition.Services;

public interface INutritionReadService
{
    Task<MealResponse> GetMealAsync(Guid userId, Guid mealId, CancellationToken cancellationToken);
    Task<MealPageResponse> GetMealsAsync(Guid userId, MealHistoryFilter filter, CancellationToken cancellationToken);
    Task<DailyNutritionSummaryResponse> GetDailyAsync(Guid userId, DateOnly date, CancellationToken cancellationToken);
    Task<NutritionRangeSummaryResponse> GetSummaryAsync(Guid userId, NutritionSummaryFilter filter,
        CancellationToken cancellationToken);
}

public class NutritionReadService : INutritionReadService
{
    private readonly FitspireDbContext _context;
    private readonly IMapper _mapper;
    private readonly INutritionTimeZoneService _timeZoneService;
    private readonly INutritionTargetService _targetService;

    public NutritionReadService(FitspireDbContext context, IMapper mapper, INutritionTimeZoneService timeZoneService,
        INutritionTargetService targetService)
    {
        _context = context;
        _mapper = mapper;
        _timeZoneService = timeZoneService;
        _targetService = targetService;
    }

    public async Task<MealResponse> GetMealAsync(Guid userId, Guid mealId, CancellationToken cancellationToken)
    {
        var meal = await ActiveMealsForUser(userId).FirstOrDefaultAsync(candidate => candidate.Id == mealId, cancellationToken)
            ?? throw new NotFoundException("Meal was not found.");
        return MapMeal(meal);
    }

    public async Task<MealPageResponse> GetMealsAsync(Guid userId, MealHistoryFilter filter,
        CancellationToken cancellationToken)
    {
        await EnsureFilterDatesAreNotFutureAsync(userId, filter.From, filter.To, cancellationToken);
        var query = ApplyHistoryFilter(ActiveMealsForUser(userId), filter);
        var totalCount = await query.CountAsync(cancellationToken);
        var meals = await query.OrderByDescending(meal => meal.MealDate).ThenByDescending(meal => meal.ConsumedAtLocalTime)
            .ThenByDescending(meal => meal.CreatedAt).Skip((filter.Page - 1) * filter.PageSize).Take(filter.PageSize)
            .ToListAsync(cancellationToken);
        return new MealPageResponse(meals.Select(MapMeal).ToList(), filter.Page, filter.PageSize, totalCount);
    }

    public async Task<DailyNutritionSummaryResponse> GetDailyAsync(Guid userId, DateOnly date,
        CancellationToken cancellationToken)
    {
        await EnsureDateIsNotFutureAsync(userId, date, cancellationToken);
        var meals = await ActiveMealsForUser(userId).Where(meal => meal.MealDate == date)
            .OrderBy(meal => meal.ConsumedAtLocalTime).ThenBy(meal => meal.CreatedAt).ToListAsync(cancellationToken);
        var target = await _targetService.GetAsync(userId, cancellationToken);
        var totals = SumTotals(meals);
        return new DailyNutritionSummaryResponse(date, meals.Select(MapMeal).ToList(), totals, target,
            NutritionSummaryFactory.CreateProgress(totals.CaloriesKcal, target?.CaloriesKcal),
            NutritionSummaryFactory.CreateProgress(totals.ProteinGrams, target?.ProteinGrams),
            NutritionSummaryFactory.CreateProgress(totals.CarbsGrams, target?.CarbsGrams),
            NutritionSummaryFactory.CreateProgress(totals.FatGrams, target?.FatGrams));
    }

    public async Task<NutritionRangeSummaryResponse> GetSummaryAsync(Guid userId, NutritionSummaryFilter filter,
        CancellationToken cancellationToken)
    {
        var today = await _timeZoneService.GetTodayAsync(userId, cancellationToken);
        var to = filter.To ?? today;
        var from = filter.From ?? to.AddDays(-30);
        if (from > to || to > today || to.DayNumber - from.DayNumber > 366)
            throw new DomainException("Nutrition summary date range must be between zero and 366 days and cannot include future dates.");

        var meals = await ActiveMealsForUser(userId).Where(meal => meal.MealDate >= from && meal.MealDate <= to)
            .ToListAsync(cancellationToken);
        var target = await _targetService.GetAsync(userId, cancellationToken);
        return NutritionSummaryFactory.Create(from, to, meals, target);
    }

    private IQueryable<Meal> ActiveMealsForUser(Guid userId) => _context.Meals.AsNoTracking()
        .Include(meal => meal.Items.Where(item => item.DeletedAt == null))
        .Where(meal => meal.UserId == userId && meal.DeletedAt == null);

    private static IQueryable<Meal> ApplyHistoryFilter(IQueryable<Meal> query, MealHistoryFilter filter)
    {
        if (filter.From.HasValue)
            query = query.Where(meal => meal.MealDate >= filter.From.Value);
        if (filter.To.HasValue)
            query = query.Where(meal => meal.MealDate <= filter.To.Value);
        if (filter.Type.HasValue)
            query = query.Where(meal => meal.MealType == filter.Type.Value);
        return query;
    }

    private MealResponse MapMeal(Meal meal)
    {
        var totals = meal.CalculateTotals();
        return new MealResponse(meal.Id, meal.MealDate, meal.ConsumedAtLocalTime, meal.MealType, meal.Name, meal.Notes,
            _mapper.Map<IReadOnlyList<MealItemResponse>>(meal.Items.Where(item => !item.IsDeleted).OrderBy(item => item.OrderIndex)), totals.CaloriesKcal,
            totals.ProteinGrams, totals.CarbsGrams, totals.FatGrams, meal.CreatedAt, meal.UpdatedAt);
    }

    private static NutritionTotalsResponse SumTotals(IEnumerable<Meal> meals)
    {
        var totals = meals.Select(meal => meal.CalculateTotals()).ToList();
        return new NutritionTotalsResponse(totals.Sum(total => total.CaloriesKcal), totals.Sum(total => total.ProteinGrams),
            totals.Sum(total => total.CarbsGrams), totals.Sum(total => total.FatGrams));
    }

    private async Task EnsureFilterDatesAreNotFutureAsync(Guid userId, DateOnly? from, DateOnly? to,
        CancellationToken cancellationToken)
    {
        if (!from.HasValue && !to.HasValue)
            return;

        var today = await _timeZoneService.GetTodayAsync(userId, cancellationToken);
        if (from is { } fromDate && (fromDate == DateOnly.MinValue || fromDate > today) ||
            to is { } toDate && (toDate == DateOnly.MinValue || toDate > today))
        {
            throw new DomainException("Nutrition date cannot be in the future in the user's timezone.");
        }
    }

    private async Task EnsureDateIsNotFutureAsync(Guid userId, DateOnly date, CancellationToken cancellationToken)
    {
        if (date == DateOnly.MinValue || date > await _timeZoneService.GetTodayAsync(userId, cancellationToken))
            throw new DomainException("Nutrition date cannot be in the future in the user's timezone.");
    }
}

internal static class NutritionSummaryFactory
{
    public static NutritionRangeSummaryResponse Create(DateOnly from, DateOnly to, IReadOnlyList<Meal> meals,
        NutritionTargetResponse? target)
    {
        var totalsByDate = meals.GroupBy(meal => meal.MealDate).ToDictionary(group => group.Key,
            group => Sum(group));
        var totals = Sum(meals);
        var loggedDayCount = totalsByDate.Count;
        var average = loggedDayCount == 0 ? null : Divide(totals, loggedDayCount);
        var points = Enumerable.Range(0, to.DayNumber - from.DayNumber + 1).Select(offset =>
        {
            var date = from.AddDays(offset);
            return new NutritionDailyTotalPoint(date, totalsByDate.GetValueOrDefault(date) ?? Zero());
        }).ToList();

        return new NutritionRangeSummaryResponse(from, to, points.Count, loggedDayCount, totals, average, target,
            CreateProgress(average?.CaloriesKcal, target?.CaloriesKcal), CreateProgress(average?.ProteinGrams, target?.ProteinGrams),
            CreateProgress(average?.CarbsGrams, target?.CarbsGrams), CreateProgress(average?.FatGrams, target?.FatGrams), points);
    }

    private static NutritionTotalsResponse Sum(IEnumerable<Meal> meals)
    {
        var totals = meals.Select(meal => meal.CalculateTotals()).ToList();
        return new NutritionTotalsResponse(totals.Sum(total => total.CaloriesKcal), totals.Sum(total => total.ProteinGrams),
            totals.Sum(total => total.CarbsGrams), totals.Sum(total => total.FatGrams));
    }

    private static NutritionTotalsResponse Divide(NutritionTotalsResponse totals, int divisor) => new(
        totals.CaloriesKcal / divisor, totals.ProteinGrams / divisor, totals.CarbsGrams / divisor, totals.FatGrams / divisor);

    private static NutritionTotalsResponse Zero() => new(0, 0, 0, 0);

    public static NutritionTargetProgressResponse CreateProgress(decimal? total, decimal? target) =>
        !total.HasValue || !target.HasValue ? new NutritionTargetProgressResponse(target, null) :
        new NutritionTargetProgressResponse(target, decimal.Round(total.Value / target.Value * 100, 2));
}
