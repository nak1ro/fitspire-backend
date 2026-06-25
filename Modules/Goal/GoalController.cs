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

    public GoalController(
        IMediator mediator,
        IValidator<CreateGoalRequest> createGoalValidator,
        IValidator<UpdateGoalRequest> updateGoalValidator)
    {
        _mediator = mediator;
        _createGoalValidator = createGoalValidator;
        _updateGoalValidator = updateGoalValidator;
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
            request.Unit,
            request.Deadline,
            request.IsRecurring,
            request.RecurrencePattern,
            request.IsPublic,
            request.SelectedWorkoutType,
            request.SelectedExerciseId
        ));
        return CreatedAtAction(nameof(GetUserGoals), new { id = goalId }, goalId);
    }

    [HttpGet]
    public async Task<ActionResult<List<GoalResponse>>> GetUserGoals()
    {
        var userId = User.GetRequiredUserId();
        var goals = await _mediator.Send(new GetUserGoalsQuery(userId));
        return Ok(goals);
    }

    [HttpGet("{id:guid}/periods")]
    public async Task<ActionResult<List<GoalPeriodResponse>>> GetPeriods(Guid id)
    {
        return Ok(await _mediator.Send(new GetGoalPeriodsQuery(User.GetRequiredUserId(), id)));
    }

    [HttpPatch("{id:guid}")]
    public async Task<IActionResult> UpdateGoal(Guid id, [FromBody] UpdateGoalRequest request)
    {
        await _updateGoalValidator.ValidateAndThrowAsync(request);
        await _mediator.Send(new UpdateGoalCommand(User.GetRequiredUserId(), id, request.TargetValue, request.IsPublic));
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
