using backend.Modules.Goal.DTOs;
using backend.Modules.Goal.Features;
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

    public GoalController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> CreateGoal([FromBody] CreateGoalRequest request)
    {
        var userId = GetUserId();
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
        var userId = GetUserId();
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
        var userId = GetUserId();
        await _mediator.Send(new UpdateGoalProgressCommand(
            id,
            userId,
            request.Delta,
            request.Source,
            request.SourceEntityId
        ));
        return NoContent();
    }

    // Helper until Auth module is fully integrated
    private Guid GetUserId() => Guid.Parse("11111111-1111-1111-1111-111111111111");
}
