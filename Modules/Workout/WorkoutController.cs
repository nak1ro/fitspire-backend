using AutoMapper;
using backend.Modules.Workout.Commands;
using backend.Modules.Workout.DTOs;
using backend.Modules.Workout.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Modules.Workout;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WorkoutController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;

    public WorkoutController(IMediator mediator, IMapper mapper)
    {
        _mediator = mediator;
        _mapper = mapper;
    }

    [HttpPost("gym")]
    public async Task<ActionResult<Guid>> CreateGymWorkout([FromBody] CreateGymWorkoutRequest request)
    {
        var command = new CreateGymWorkoutCommand(
            request.UserId,
            request.Date,
            request.SplitType,
            request.IntensityLevel,
            request.Exercises.Select(e => new ExerciseInput(
                e.ExerciseId,
                e.Sets,
                e.Reps,
                e.WeightKg
            )).ToList()
        );

        var workoutId = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetWorkoutById), new { id = workoutId }, workoutId);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<WorkoutResponse>> GetWorkoutById(Guid id)
    {
        var workout = await _mediator.Send(new GetWorkoutByIdQuery(id));
        
        if (workout is null)
            return NotFound();

        var response = _mapper.Map<WorkoutResponse>(workout);
        return Ok(response);
    }

    [HttpPost("{id:guid}/complete")]
    public async Task<ActionResult> CompleteWorkout(Guid id, [FromBody] CompleteWorkoutRequest request)
    {
        await _mediator.Send(new CompleteWorkoutCommand(id, request.DurationMinutes));
        return Ok(new { success = true });
    }
}
