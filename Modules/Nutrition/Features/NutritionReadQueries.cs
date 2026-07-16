using backend.Modules.Nutrition.Contracts;
using backend.Modules.Nutrition.Services;
using MediatR;

namespace backend.Modules.Nutrition.Features;

public record GetMealQuery(Guid UserId, Guid MealId) : IRequest<MealResponse>;
public record GetMealHistoryQuery(Guid UserId, MealHistoryFilter Filter) : IRequest<MealPageResponse>;
public record GetDailyNutritionQuery(Guid UserId, DateOnly Date) : IRequest<DailyNutritionSummaryResponse>;
public record GetNutritionSummaryQuery(Guid UserId, NutritionSummaryFilter Filter) : IRequest<NutritionRangeSummaryResponse>;

public class GetMealHandler : IRequestHandler<GetMealQuery, MealResponse>
{
    private readonly INutritionReadService _service;
    public GetMealHandler(INutritionReadService service) => _service = service;
    public Task<MealResponse> Handle(GetMealQuery query, CancellationToken cancellationToken) =>
        _service.GetMealAsync(query.UserId, query.MealId, cancellationToken);
}

public class GetMealHistoryHandler : IRequestHandler<GetMealHistoryQuery, MealPageResponse>
{
    private readonly INutritionReadService _service;
    public GetMealHistoryHandler(INutritionReadService service) => _service = service;
    public Task<MealPageResponse> Handle(GetMealHistoryQuery query, CancellationToken cancellationToken) =>
        _service.GetMealsAsync(query.UserId, query.Filter, cancellationToken);
}

public class GetDailyNutritionHandler : IRequestHandler<GetDailyNutritionQuery, DailyNutritionSummaryResponse>
{
    private readonly INutritionReadService _service;
    public GetDailyNutritionHandler(INutritionReadService service) => _service = service;
    public Task<DailyNutritionSummaryResponse> Handle(GetDailyNutritionQuery query, CancellationToken cancellationToken) =>
        _service.GetDailyAsync(query.UserId, query.Date, cancellationToken);
}

public class GetNutritionSummaryHandler : IRequestHandler<GetNutritionSummaryQuery, NutritionRangeSummaryResponse>
{
    private readonly INutritionReadService _service;
    public GetNutritionSummaryHandler(INutritionReadService service) => _service = service;
    public Task<NutritionRangeSummaryResponse> Handle(GetNutritionSummaryQuery query, CancellationToken cancellationToken) =>
        _service.GetSummaryAsync(query.UserId, query.Filter, cancellationToken);
}
