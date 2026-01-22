using backend.Modules.Workout.Commands;
using backend.Modules.Workout.Domain;
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

    public WorkoutController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Creates a new gym workout.
    /// </summary>
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

    /// <summary>
    /// Gets a workout by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<UserWorkout>> GetWorkoutById(Guid id)
    {
        var workout = await _mediator.Send(new GetWorkoutByIdQuery(id));
        
        if (workout is null)
            return NotFound();

        return Ok(workout);
    }

    /// <summary>
    /// Marks a workout as completed.
    /// </summary>
    [HttpPost("{id:guid}/complete")]
    public async Task<ActionResult> CompleteWorkout(Guid id, [FromBody] CompleteWorkoutRequest request)
    {
        var result = await _mediator.Send(new CompleteWorkoutCommand(id, request.DurationMinutes));
        return Ok(new { success = result });
    }
}

// Request DTOs
public record CreateGymWorkoutRequest(
    Guid UserId,
    DateTime Date,
    string? SplitType,
    string? IntensityLevel,
    List<ExerciseInputRequest> Exercises
);

public record ExerciseInputRequest(
    Guid ExerciseId,
    int Sets,
    int Reps,
    double WeightKg
);

public record CompleteWorkoutRequest(
    double? DurationMinutes
);
