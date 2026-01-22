using AutoMapper;
using backend.Modules.Workout.Features.Common;
using backend.Modules.Workout.Features.GymWorkout;
using backend.Modules.Workout.Features.RunningWorkout;
using backend.Modules.Workout.Features.CyclingWorkout;
using backend.Modules.Workout.DTOs;
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

    [HttpPost("running")]
    public async Task<ActionResult<RunningWorkoutResponse>> CreateRunningWorkout([FromBody] CreateRunningWorkoutRequest request)
    {
        var command = new CreateRunningWorkoutCommand(
            request.UserId,
            request.Date,
            request.DistanceKm,
            request.DurationMinutes,
            request.ElevationGainMeters,
            request.StepCount,
            request.CaloriesBurned,
            request.MapData,
            request.Notes,
            request.IsPrivate
        );
        
        var workoutId = await _mediator.Send(command);
        
        // We get the workout back to return the full response
        var workout = await _mediator.Send(new GetWorkoutByIdQuery(workoutId));
        return Ok(_mapper.Map<RunningWorkoutResponse>(workout));
    }

    [HttpPost("cycling")]
    public async Task<ActionResult<CyclingWorkoutResponse>> CreateCyclingWorkout([FromBody] CreateCyclingWorkoutRequest request)
    {
        var command = new CreateCyclingWorkoutCommand(
            request.UserId,
            request.Date,
            request.DistanceKm,
            request.DurationMinutes,
            request.ElevationGainMeters,
            request.CaloriesBurned,
            request.MapData,
            request.Notes,
            request.IsPrivate,
            request.IsIndoor
        );
        
        var workoutId = await _mediator.Send(command);
        
        var workout = await _mediator.Send(new GetWorkoutByIdQuery(workoutId));
        return Ok(_mapper.Map<CyclingWorkoutResponse>(workout));
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
