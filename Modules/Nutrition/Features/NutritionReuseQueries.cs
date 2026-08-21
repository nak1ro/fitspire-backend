using backend.Modules.Nutrition.Contracts;
using backend.Modules.Nutrition.Services;
using MediatR;

namespace backend.Modules.Nutrition.Features;

public record GetNutritionTargetQuery(Guid UserId) : IRequest<NutritionTargetResponse?>;
public record GetFavouriteFoodsQuery(Guid UserId, FavouriteFoodFilter Filter) : IRequest<FavouriteFoodPageResponse>;
public record GetRecentFoodsQuery(Guid UserId, RecentFoodsFilter Filter) : IRequest<IReadOnlyList<RecentFoodResponse>>;
public record GetCommonFoodsQuery(CommonFoodFilter Filter) : IRequest<IReadOnlyList<CommonFoodResponse>>;

public class GetNutritionTargetHandler : IRequestHandler<GetNutritionTargetQuery, NutritionTargetResponse?>
{
    private readonly INutritionTargetService _service;
    public GetNutritionTargetHandler(INutritionTargetService service) => _service = service;
    public Task<NutritionTargetResponse?> Handle(GetNutritionTargetQuery query, CancellationToken cancellationToken) =>
        _service.GetAsync(query.UserId, cancellationToken);
}

public class GetFavouriteFoodsHandler : IRequestHandler<GetFavouriteFoodsQuery, FavouriteFoodPageResponse>
{
    private readonly IFavouriteFoodService _service;
    public GetFavouriteFoodsHandler(IFavouriteFoodService service) => _service = service;
    public Task<FavouriteFoodPageResponse> Handle(GetFavouriteFoodsQuery query, CancellationToken cancellationToken) =>
        _service.GetPageAsync(query.UserId, query.Filter, cancellationToken);
}

public class GetRecentFoodsHandler : IRequestHandler<GetRecentFoodsQuery, IReadOnlyList<RecentFoodResponse>>
{
    private readonly IRecentFoodService _service;
    public GetRecentFoodsHandler(IRecentFoodService service) => _service = service;
    public Task<IReadOnlyList<RecentFoodResponse>> Handle(GetRecentFoodsQuery query, CancellationToken cancellationToken) =>
        _service.GetAsync(query.UserId, query.Filter, cancellationToken);
}

public class GetCommonFoodsHandler : IRequestHandler<GetCommonFoodsQuery, IReadOnlyList<CommonFoodResponse>>
{
    private readonly ICommonFoodService _service;
    public GetCommonFoodsHandler(ICommonFoodService service) => _service = service;
    public Task<IReadOnlyList<CommonFoodResponse>> Handle(GetCommonFoodsQuery query, CancellationToken cancellationToken) =>
        _service.GetAsync(query.Filter, cancellationToken);
}
