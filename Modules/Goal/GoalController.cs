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
    private readonly IValidator<UpdateGoalProgressRequest> _updateGoalProgressValidator;

    public GoalController(
        IMediator mediator,
        IValidator<CreateGoalRequest> createGoalValidator,
        IValidator<UpdateGoalProgressRequest> updateGoalProgressValidator)
    {
        _mediator = mediator;
        _createGoalValidator = createGoalValidator;
        _updateGoalProgressValidator = updateGoalProgressValidator;
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
            request.IsPublic
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

    [HttpGet("types")]
    public async Task<ActionResult<List<GoalTypeResponse>>> GetGoalTypes()
    {
        var types = await _mediator.Send(new GetGoalTypesQuery());
        return Ok(types);
    }

    [HttpPost("{id:guid}/progress")]
    public async Task<IActionResult> UpdateProgress(Guid id, [FromBody] UpdateGoalProgressRequest request)
    {
        await _updateGoalProgressValidator.ValidateAndThrowAsync(request);

        var userId = User.GetRequiredUserId();
        await _mediator.Send(new UpdateGoalProgressCommand(
            id,
            userId,
            request.Delta,
            request.Source,
            request.SourceEntityId
        ));
        return NoContent();
    }
}
