using backend.Modules.Goal.DTOs;
using backend.Modules.Goal.Features;
using backend.Modules.Shared.Extensions;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Modules.Goal;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class GoalController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IValidator<CreateGoalRequest> _createGoalValidator;
    private readonly IValidator<UpdateGoalRequest> _updateGoalValidator;
    private readonly IValidator<GoalListFilter> _listFilterValidator;
    private readonly IValidator<GoalPagination> _paginationValidator;

    public GoalController(
        IMediator mediator,
        IValidator<CreateGoalRequest> createGoalValidator,
        IValidator<UpdateGoalRequest> updateGoalValidator,
        IValidator<GoalListFilter> listFilterValidator,
        IValidator<GoalPagination> paginationValidator)
    {
        _mediator = mediator;
        _createGoalValidator = createGoalValidator;
        _updateGoalValidator = updateGoalValidator;
        _listFilterValidator = listFilterValidator;
        _paginationValidator = paginationValidator;
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> CreateGoal([FromBody] CreateGoalRequest request)
    {
        await _createGoalValidator.ValidateAndThrowAsync(request);

        var userId = User.GetRequiredUserId();
        var goalId = await _mediator.Send(new CreateGoalCommand(
            userId,
            request.GoalTypeId,
            request.TargetValue,
            request.Schedule,
            request.Deadline,
            request.IsPublic,
            request.SelectedWorkoutType,
            request.SelectedExerciseId,
            request.StartDate
        ));
        return CreatedAtAction(nameof(GetGoal), new { id = goalId }, goalId);
    }

    [HttpGet]
    public async Task<ActionResult<GoalPageResponse<GoalResponse>>> GetUserGoals([FromQuery] GoalListFilter filter)
    {
        await _listFilterValidator.ValidateAndThrowAsync(filter);
        var goals = await _mediator.Send(new GetUserGoalsQuery(User.GetRequiredUserId(), filter));
        return Ok(goals);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<GoalDetailResponse>> GetGoal(Guid id) =>
        Ok(await _mediator.Send(new GetGoalDetailQuery(User.GetRequiredUserId(), id)));

    [HttpGet("{id:guid}/periods")]
    public async Task<ActionResult<GoalPageResponse<GoalPeriodResponse>>> GetPeriods(Guid id, [FromQuery] GoalPagination pagination)
    {
        await _paginationValidator.ValidateAndThrowAsync(pagination);
        return Ok(await _mediator.Send(new GetGoalPeriodsQuery(User.GetRequiredUserId(), id, pagination)));
    }

    [HttpGet("{id:guid}/progress")]
    public async Task<ActionResult<GoalPageResponse<GoalProgressEntryResponse>>> GetProgress(Guid id, [FromQuery] GoalPagination pagination)
    {
        await _paginationValidator.ValidateAndThrowAsync(pagination);
        return Ok(await _mediator.Send(new GetGoalProgressQuery(User.GetRequiredUserId(), id, pagination)));
    }

    [HttpGet("{id:guid}/target-history")]
    public async Task<ActionResult<GoalPageResponse<GoalTargetChangeResponse>>> GetTargetHistory(Guid id, [FromQuery] GoalPagination pagination)
    {
        await _paginationValidator.ValidateAndThrowAsync(pagination);
        return Ok(await _mediator.Send(new GetGoalTargetChangesQuery(User.GetRequiredUserId(), id, pagination)));
    }

    [HttpPatch("{id:guid}")]
    public async Task<IActionResult> UpdateGoal(Guid id, [FromBody] UpdateGoalRequest request)
    {
        await _updateGoalValidator.ValidateAndThrowAsync(request);
        await _mediator.Send(new UpdateGoalCommand(User.GetRequiredUserId(), id, request.TargetValue, request.IsPublic, request.Deadline));
        return NoContent();
    }

    [HttpPost("{id:guid}/archive")]
    public async Task<IActionResult> ArchiveGoal(Guid id)
    {
        await _mediator.Send(new ArchiveGoalCommand(User.GetRequiredUserId(), id));
        return NoContent();
    }

    [HttpGet("types")]
    public async Task<ActionResult<List<GoalTypeResponse>>> GetGoalTypes()
    {
        var types = await _mediator.Send(new GetGoalTypesQuery());
        return Ok(types);
    }

}
