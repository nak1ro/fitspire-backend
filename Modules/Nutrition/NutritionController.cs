using backend.Modules.Nutrition.Contracts;
using backend.Modules.Nutrition.Features;
using backend.Modules.Shared.Extensions;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Modules.Nutrition;

[ApiController]
[Route("api/nutrition")]
[Authorize]
public class NutritionController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IValidator<MealHistoryFilter> _historyValidator;
    private readonly IValidator<NutritionSummaryFilter> _summaryValidator;
    private readonly IValidator<FavouriteFoodFilter> _favouriteValidator;
    private readonly IValidator<RecentFoodsFilter> _recentValidator;

    public NutritionController(IMediator mediator, IValidator<MealHistoryFilter> historyValidator,
        IValidator<NutritionSummaryFilter> summaryValidator, IValidator<FavouriteFoodFilter> favouriteValidator,
        IValidator<RecentFoodsFilter> recentValidator)
    {
        _mediator = mediator;
        _historyValidator = historyValidator;
        _summaryValidator = summaryValidator;
        _favouriteValidator = favouriteValidator;
        _recentValidator = recentValidator;
    }

    [HttpPost("meals")]
    public async Task<ActionResult<Guid>> CreateMeal(CreateMealRequest request, CancellationToken cancellationToken)
    {
        var id = await _mediator.Send(new CreateMealCommand(User.GetRequiredUserId(), request), cancellationToken);
        return CreatedAtAction(nameof(GetMeal), new { id }, id);
    }

    [HttpGet("meals/{id:guid}")]
    public Task<MealResponse> GetMeal(Guid id, CancellationToken cancellationToken) =>
        _mediator.Send(new GetMealQuery(User.GetRequiredUserId(), id), cancellationToken);

    [HttpGet("meals")]
    public async Task<ActionResult<MealPageResponse>> GetMeals([FromQuery] MealHistoryFilter filter,
        CancellationToken cancellationToken)
    {
        await _historyValidator.ValidateAndThrowAsync(filter, cancellationToken);
        return Ok(await _mediator.Send(new GetMealHistoryQuery(User.GetRequiredUserId(), filter), cancellationToken));
    }

    [HttpPut("meals/{id:guid}")]
    public async Task<IActionResult> UpdateMeal(Guid id, UpdateMealRequest request, CancellationToken cancellationToken)
    {
        await _mediator.Send(new UpdateMealCommand(User.GetRequiredUserId(), id, request), cancellationToken);
        return NoContent();
    }

    [HttpDelete("meals/{id:guid}")]
    public async Task<IActionResult> DeleteMeal(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteMealCommand(User.GetRequiredUserId(), id), cancellationToken);
        return NoContent();
    }

    [HttpPost("meals/{mealId:guid}/items")]
    public async Task<ActionResult<Guid>> AddItem(Guid mealId, AddMealItemRequest request, CancellationToken cancellationToken)
    {
        var id = await _mediator.Send(new AddMealItemCommand(User.GetRequiredUserId(), mealId, request), cancellationToken);
        return Ok(id);
    }

    [HttpPut("meals/{mealId:guid}/items/{itemId:guid}")]
    public async Task<IActionResult> UpdateItem(Guid mealId, Guid itemId, MealItemRequest request,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(new UpdateMealItemCommand(User.GetRequiredUserId(), mealId, itemId, request), cancellationToken);
        return NoContent();
    }

    [HttpDelete("meals/{mealId:guid}/items/{itemId:guid}")]
    public async Task<IActionResult> DeleteItem(Guid mealId, Guid itemId, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteMealItemCommand(User.GetRequiredUserId(), mealId, itemId), cancellationToken);
        return NoContent();
    }

    [HttpPut("meals/{mealId:guid}/items/order")]
    public async Task<IActionResult> ReorderItems(Guid mealId, ReorderMealItemsRequest request,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(new ReorderMealItemsCommand(User.GetRequiredUserId(), mealId, request), cancellationToken);
        return NoContent();
    }

    [HttpGet("target")]
    public async Task<ActionResult<NutritionTargetResponse>> GetTarget(CancellationToken cancellationToken)
    {
        var target = await _mediator.Send(new GetNutritionTargetQuery(User.GetRequiredUserId()), cancellationToken);
        return target is null ? NoContent() : Ok(target);
    }

    [HttpPut("target")]
    public async Task<IActionResult> UpsertTarget(NutritionTargetRequest request, CancellationToken cancellationToken)
    {
        await _mediator.Send(new UpsertNutritionTargetCommand(User.GetRequiredUserId(), request), cancellationToken);
        return NoContent();
    }

    [HttpDelete("target")]
    public async Task<IActionResult> DeleteTarget(CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteNutritionTargetCommand(User.GetRequiredUserId()), cancellationToken);
        return NoContent();
    }

    [HttpPost("favourites")]
    public async Task<ActionResult<Guid>> CreateFavourite(FavouriteFoodRequest request, CancellationToken cancellationToken)
    {
        var id = await _mediator.Send(new CreateFavouriteFoodCommand(User.GetRequiredUserId(), request), cancellationToken);
        return Ok(id);
    }

    [HttpGet("favourites")]
    public async Task<ActionResult<FavouriteFoodPageResponse>> GetFavourites([FromQuery] FavouriteFoodFilter filter,
        CancellationToken cancellationToken)
    {
        await _favouriteValidator.ValidateAndThrowAsync(filter, cancellationToken);
        return Ok(await _mediator.Send(new GetFavouriteFoodsQuery(User.GetRequiredUserId(), filter), cancellationToken));
    }

    [HttpPut("favourites/{id:guid}")]
    public async Task<IActionResult> UpdateFavourite(Guid id, FavouriteFoodRequest request, CancellationToken cancellationToken)
    {
        await _mediator.Send(new UpdateFavouriteFoodCommand(User.GetRequiredUserId(), id, request), cancellationToken);
        return NoContent();
    }

    [HttpDelete("favourites/{id:guid}")]
    public async Task<IActionResult> DeleteFavourite(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteFavouriteFoodCommand(User.GetRequiredUserId(), id), cancellationToken);
        return NoContent();
    }

    [HttpGet("recent")]
    public async Task<ActionResult<IReadOnlyList<RecentFoodResponse>>> GetRecent([FromQuery] RecentFoodsFilter filter,
        CancellationToken cancellationToken)
    {
        await _recentValidator.ValidateAndThrowAsync(filter, cancellationToken);
        return Ok(await _mediator.Send(new GetRecentFoodsQuery(User.GetRequiredUserId(), filter), cancellationToken));
    }

    [HttpGet("daily/{date}")]
    public Task<DailyNutritionSummaryResponse> GetDaily(DateOnly date, CancellationToken cancellationToken) =>
        _mediator.Send(new GetDailyNutritionQuery(User.GetRequiredUserId(), date), cancellationToken);

    [HttpGet("summary")]
    public async Task<ActionResult<NutritionRangeSummaryResponse>> GetSummary([FromQuery] NutritionSummaryFilter filter,
        CancellationToken cancellationToken)
    {
        await _summaryValidator.ValidateAndThrowAsync(filter, cancellationToken);
        return Ok(await _mediator.Send(new GetNutritionSummaryQuery(User.GetRequiredUserId(), filter), cancellationToken));
    }
}
