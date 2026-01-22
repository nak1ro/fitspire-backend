using AutoMapper;
using backend.Modules.Workout.Features.Common;
using backend.Modules.Workout.Features.GymWorkout;
using backend.Modules.Workout.DTOs;
using backend.Modules.Workout.Features.Common;
using backend.Modules.Workout.Features.RunningWorkout;
using backend.Modules.Workout.Features.CyclingWorkout;
using backend.Modules.Workout.Features.SwimmingWorkout;
using backend.Modules.Workout.Features.YogaWorkout;
using backend.Modules.Workout.Domain.Enums;
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

    [HttpPost("yoga")]
    public async Task<ActionResult<YogaWorkoutResponse>> CreateYogaWorkout([FromBody] CreateYogaWorkoutRequest request)
    {
        YogaStyle? style = !string.IsNullOrEmpty(request.Style) 
            ? Enum.Parse<YogaStyle>(request.Style, true) : null;
            
        YogaIntensity? intensity = !string.IsNullOrEmpty(request.Intensity) 
            ? Enum.Parse<YogaIntensity>(request.Intensity, true) : null;
            
        YogaFocusArea? focusArea = !string.IsNullOrEmpty(request.FocusArea) 
            ? Enum.Parse<YogaFocusArea>(request.FocusArea, true) : null;

        var command = new CreateYogaWorkoutCommand(
            request.UserId,
            request.Date,
            style,
            intensity,
            focusArea,
            request.DurationMinutes,
            request.CaloriesBurned,
            request.Notes,
            request.IsPrivate
        );
        
        var workoutId = await _mediator.Send(command);
        
        var workout = await _mediator.Send(new GetWorkoutByIdQuery(workoutId));
        return Ok(_mapper.Map<YogaWorkoutResponse>(workout));
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

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateWorkout(Guid id, [FromBody] UpdateWorkoutRequest request)
    {
        var userId = GetUserId();
        await _mediator.Send(new UpdateWorkoutCommand(id, userId, request));
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteWorkout(Guid id)
    {
        var userId = GetUserId();
        await _mediator.Send(new DeleteWorkoutCommand(id, userId));
        return NoContent();
    }

    [HttpGet]
    public async Task<IActionResult> GetWorkouts([FromQuery] WorkoutFilterRequest filter)
    {
        var userId = GetUserId();
        var result = await _mediator.Send(new GetWorkoutsQuery(userId, filter));
        return Ok(result);
    }

    [HttpPost("{id:guid}/save-as-routine")]
    public async Task<ActionResult<Guid>> SaveAsRoutine(Guid id, [FromBody] SaveRoutineRequest request)
    {
        var userId = GetUserId();
        var routineId = await _mediator.Send(new SaveWorkoutAsRoutineCommand(userId, id, request.Name, request.Description));
        return Ok(routineId);
    }

    [HttpPost("from-routine/{routineId:guid}")]
    public async Task<ActionResult<Guid>> CreateFromRoutine(Guid routineId, [FromBody] CreateFromRoutineRequest request)
    {
        var userId = GetUserId();
        var workoutId = await _mediator.Send(new CreateWorkoutFromRoutineCommand(userId, routineId, request.Date));
        return CreatedAtAction(nameof(GetWorkoutById), new { id = workoutId }, workoutId);
    }
    
    // Helper until Auth module is fully integrated
    private Guid GetUserId() => Guid.Parse("11111111-1111-1111-1111-111111111111");
}

public record SaveRoutineRequest(string Name, string? Description);
public record CreateFromRoutineRequest(DateTime Date);
