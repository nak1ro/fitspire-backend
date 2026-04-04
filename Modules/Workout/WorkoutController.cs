using AutoMapper;
using backend.Modules.Shared.Extensions;
using backend.Modules.Workout.DTOs;
using backend.Modules.Workout.Features.Common;
using backend.Modules.Workout.Features.GymWorkout;
using backend.Modules.Workout.Features.RunningWorkout;
using backend.Modules.Workout.Features.CyclingWorkout;
using backend.Modules.Workout.Features.SwimmingWorkout;
using backend.Modules.Workout.Features.YogaWorkout;
using backend.Modules.Workout.Domain.Enums;
using FluentValidation;
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
    private readonly IValidator<CreateGymWorkoutRequest> _createGymValidator;
    private readonly IValidator<CreateRunningWorkoutRequest> _createRunningValidator;
    private readonly IValidator<CreateCyclingWorkoutRequest> _createCyclingValidator;
    private readonly IValidator<CreateYogaWorkoutRequest> _createYogaValidator;
    private readonly IValidator<CreateSwimmingWorkoutRequest> _createSwimmingValidator;
    private readonly IValidator<CompleteWorkoutRequest> _completeWorkoutValidator;
    private readonly IValidator<UpdateWorkoutRequest> _updateWorkoutValidator;
    private readonly IValidator<WorkoutFilterRequest> _workoutFilterValidator;
    private readonly IValidator<SaveRoutineRequest> _saveRoutineValidator;
    private readonly IValidator<CreateFromRoutineRequest> _createFromRoutineValidator;

    public WorkoutController(
        IMediator mediator,
        IMapper mapper,
        IValidator<CreateGymWorkoutRequest> createGymValidator,
        IValidator<CreateRunningWorkoutRequest> createRunningValidator,
        IValidator<CreateCyclingWorkoutRequest> createCyclingValidator,
        IValidator<CreateYogaWorkoutRequest> createYogaValidator,
        IValidator<CreateSwimmingWorkoutRequest> createSwimmingValidator,
        IValidator<CompleteWorkoutRequest> completeWorkoutValidator,
        IValidator<UpdateWorkoutRequest> updateWorkoutValidator,
        IValidator<WorkoutFilterRequest> workoutFilterValidator,
        IValidator<SaveRoutineRequest> saveRoutineValidator,
        IValidator<CreateFromRoutineRequest> createFromRoutineValidator)
    {
        _mediator = mediator;
        _mapper = mapper;
        _createGymValidator = createGymValidator;
        _createRunningValidator = createRunningValidator;
        _createCyclingValidator = createCyclingValidator;
        _createYogaValidator = createYogaValidator;
        _createSwimmingValidator = createSwimmingValidator;
        _completeWorkoutValidator = completeWorkoutValidator;
        _updateWorkoutValidator = updateWorkoutValidator;
        _workoutFilterValidator = workoutFilterValidator;
        _saveRoutineValidator = saveRoutineValidator;
        _createFromRoutineValidator = createFromRoutineValidator;
    }

    [HttpPost("gym")]
    public async Task<ActionResult<Guid>> CreateGymWorkout([FromBody] CreateGymWorkoutRequest request)
    {
        await _createGymValidator.ValidateAndThrowAsync(request);

        var userId = User.GetRequiredUserId();
        var command = new CreateGymWorkoutCommand(
            userId,
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
        await _createRunningValidator.ValidateAndThrowAsync(request);

        var userId = User.GetRequiredUserId();
        var command = new CreateRunningWorkoutCommand(
            userId,
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
        
        var workout = await _mediator.Send(new GetWorkoutByIdQuery(workoutId, userId));
        return Ok(_mapper.Map<RunningWorkoutResponse>(workout));
    }

    [HttpPost("cycling")]
    public async Task<ActionResult<CyclingWorkoutResponse>> CreateCyclingWorkout([FromBody] CreateCyclingWorkoutRequest request)
    {
        await _createCyclingValidator.ValidateAndThrowAsync(request);

        var userId = User.GetRequiredUserId();
        var command = new CreateCyclingWorkoutCommand(
            userId,
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
        
        var workout = await _mediator.Send(new GetWorkoutByIdQuery(workoutId, userId));
        return Ok(_mapper.Map<CyclingWorkoutResponse>(workout));
    }

    [HttpPost("yoga")]
    public async Task<ActionResult<YogaWorkoutResponse>> CreateYogaWorkout([FromBody] CreateYogaWorkoutRequest request)
    {
        await _createYogaValidator.ValidateAndThrowAsync(request);

        var userId = User.GetRequiredUserId();
        YogaStyle? style = !string.IsNullOrEmpty(request.Style) 
            ? Enum.Parse<YogaStyle>(request.Style, true) : null;
            
        YogaIntensity? intensity = !string.IsNullOrEmpty(request.Intensity) 
            ? Enum.Parse<YogaIntensity>(request.Intensity, true) : null;
            
        YogaFocusArea? focusArea = !string.IsNullOrEmpty(request.FocusArea) 
            ? Enum.Parse<YogaFocusArea>(request.FocusArea, true) : null;

        var command = new CreateYogaWorkoutCommand(
            userId,
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
        
        var workout = await _mediator.Send(new GetWorkoutByIdQuery(workoutId, userId));
        return Ok(_mapper.Map<YogaWorkoutResponse>(workout));
    }

    [HttpPost("swimming")]
    public async Task<ActionResult<SwimmingWorkoutResponse>> CreateSwimmingWorkout([FromBody] CreateSwimmingWorkoutRequest request)
    {
        await _createSwimmingValidator.ValidateAndThrowAsync(request);

        var userId = User.GetRequiredUserId();
        var command = new CreateSwimmingWorkoutCommand(
            userId,
            request.Date,
            request.Laps,
            request.PoolLengthMeters,
            request.DistanceMeters,
            request.StrokeType,
            request.DurationMinutes,
            request.CaloriesBurned,
            request.Notes,
            request.IsPrivate
        );

        var workoutId = await _mediator.Send(command);

        var workout = await _mediator.Send(new GetWorkoutByIdQuery(workoutId, userId));
        return Ok(_mapper.Map<SwimmingWorkoutResponse>(workout));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<WorkoutResponse>> GetWorkoutById(Guid id)
    {
        var userId = User.GetRequiredUserId();
        var workout = await _mediator.Send(new GetWorkoutByIdQuery(id, userId));
        
        if (workout is null)
            return NotFound();

        var response = _mapper.Map<WorkoutResponse>(workout);
        return Ok(response);
    }

    [HttpPost("{id:guid}/complete")]
    public async Task<ActionResult> CompleteWorkout(Guid id, [FromBody] CompleteWorkoutRequest request)
    {
        await _completeWorkoutValidator.ValidateAndThrowAsync(request);

        var userId = User.GetRequiredUserId();
        await _mediator.Send(new CompleteWorkoutCommand(id, userId, request.DurationMinutes));
        return Ok(new { success = true });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateWorkout(Guid id, [FromBody] UpdateWorkoutRequest request)
    {
        await _updateWorkoutValidator.ValidateAndThrowAsync(request);

        var userId = User.GetRequiredUserId();
        await _mediator.Send(new UpdateWorkoutCommand(id, userId, request));
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteWorkout(Guid id)
    {
        var userId = User.GetRequiredUserId();
        await _mediator.Send(new DeleteWorkoutCommand(id, userId));
        return NoContent();
    }

    [HttpGet]
    public async Task<IActionResult> GetWorkouts([FromQuery] WorkoutFilterRequest filter)
    {
        await _workoutFilterValidator.ValidateAndThrowAsync(filter);

        var userId = User.GetRequiredUserId();
        var result = await _mediator.Send(new GetWorkoutsQuery(userId, filter));
        return Ok(result);
    }

    [HttpGet("exercise-categories")]
    public async Task<ActionResult<List<ExerciseCategoryResponse>>> GetExerciseCategories()
    {
        var categories = await _mediator.Send(new GetExerciseCategoriesQuery());
        return Ok(categories);
    }

    [HttpGet("exercises")]
    public async Task<ActionResult<List<ExerciseResponse>>> GetExercises(
        [FromQuery] Guid? categoryId,
        [FromQuery] string? search)
    {
        var exercises = await _mediator.Send(new GetExercisesQuery(categoryId, search));
        return Ok(exercises);
    }

    [HttpGet("routines")]
    public async Task<ActionResult<List<WorkoutRoutineResponse>>> GetRoutines()
    {
        var userId = User.GetRequiredUserId();
        var routines = await _mediator.Send(new GetWorkoutRoutinesQuery(userId));
        return Ok(routines);
    }

    [HttpGet("personal-records")]
    public async Task<ActionResult<List<PersonalRecordResponse>>> GetPersonalRecords()
    {
        var userId = User.GetRequiredUserId();
        var records = await _mediator.Send(new GetPersonalRecordsQuery(userId));
        return Ok(records);
    }

    [HttpGet("routines/{routineId:guid}")]
    public async Task<ActionResult<WorkoutRoutineResponse>> GetRoutine(Guid routineId)
    {
        var userId = User.GetRequiredUserId();
        var routine = await _mediator.Send(new GetWorkoutRoutineByIdQuery(userId, routineId));
        return Ok(routine);
    }

    [HttpPost("{id:guid}/save-as-routine")]
    public async Task<ActionResult<Guid>> SaveAsRoutine(Guid id, [FromBody] SaveRoutineRequest request)
    {
        await _saveRoutineValidator.ValidateAndThrowAsync(request);

        var userId = User.GetRequiredUserId();
        var routineId = await _mediator.Send(new SaveWorkoutAsRoutineCommand(userId, id, request.Name, request.Description));
        return Ok(routineId);
    }

    [HttpPost("from-routine/{routineId:guid}")]
    public async Task<ActionResult<Guid>> CreateFromRoutine(Guid routineId, [FromBody] CreateFromRoutineRequest request)
    {
        await _createFromRoutineValidator.ValidateAndThrowAsync(request);

        var userId = User.GetRequiredUserId();
        var workoutId = await _mediator.Send(new CreateWorkoutFromRoutineCommand(userId, routineId, request.Date));
        return CreatedAtAction(nameof(GetWorkoutById), new { id = workoutId }, workoutId);
    }

    [HttpDelete("routines/{routineId:guid}")]
    public async Task<IActionResult> DeleteRoutine(Guid routineId)
    {
        var userId = User.GetRequiredUserId();
        await _mediator.Send(new DeleteWorkoutRoutineCommand(userId, routineId));
        return NoContent();
    }
}
