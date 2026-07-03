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
using backend.Modules.Workout.Domain.Entities;
using backend.Modules.Workout.Features.Sessions;
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
    private readonly IValidator<UpdateRoutineRequest> _updateRoutineValidator;

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
        IValidator<CreateFromRoutineRequest> createFromRoutineValidator,
        IValidator<UpdateRoutineRequest> updateRoutineValidator)
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
        _updateRoutineValidator = updateRoutineValidator;
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
                e.Sets.Select(set => new SetInput(
                    set.Reps,
                    set.WeightKg,
                    set.DurationSeconds,
                    set.DistanceMeters,
                    set.IsWarmup,
                    set.Rpe,
                    set.Notes,
                    set.IsCompleted)).ToList(),
                e.Notes
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
    public async Task<ActionResult<object>> GetWorkoutById(Guid id)
    {
        var userId = User.GetRequiredUserId();
        var workout = await _mediator.Send(new GetWorkoutByIdQuery(id, userId));
        
        if (workout is null)
            return NotFound();

        return Ok(MapWorkoutDetails(workout));
    }

    [HttpGet("sessions/active")]
    public async Task<ActionResult<WorkoutSessionResponse>> GetActiveSession()
    {
        var session = await _mediator.Send(new GetActiveWorkoutSessionQuery(User.GetRequiredUserId()));
        return session is null ? NoContent() : Ok(session);
    }

    private object MapWorkoutDetails(UserWorkout workout)
    {
        return workout switch
        {
            GymUserWorkoutDetails gymWorkout => _mapper.Map<GymWorkoutResponse>(gymWorkout),
            RunningUserWorkoutDetails runningWorkout => _mapper.Map<RunningWorkoutResponse>(runningWorkout),
            CyclingUserWorkoutDetails cyclingWorkout => _mapper.Map<CyclingWorkoutResponse>(cyclingWorkout),
            SwimmingUserWorkoutDetails swimmingWorkout => _mapper.Map<SwimmingWorkoutResponse>(swimmingWorkout),
            YogaUserWorkoutDetails yogaWorkout => _mapper.Map<YogaWorkoutResponse>(yogaWorkout),
            _ => _mapper.Map<WorkoutResponse>(workout)
        };
    }

    [HttpPost("{id:guid}/complete")]
    public async Task<ActionResult> CompleteWorkout(Guid id, [FromBody] CompleteWorkoutRequest request)
    {
        await _completeWorkoutValidator.ValidateAndThrowAsync(request);

        var userId = User.GetRequiredUserId();
        await _mediator.Send(new CompleteWorkoutCommand(id, userId, request.DurationMinutes, request.Notes, request.IsPrivate));
        return Ok(new { success = true });
    }

    [HttpPost("{id:guid}/finish")]
    public async Task<ActionResult> FinishWorkout(Guid id, [FromBody] CompleteWorkoutRequest request)
    {
        await _completeWorkoutValidator.ValidateAndThrowAsync(request);

        var userId = User.GetRequiredUserId();
        await _mediator.Send(new CompleteWorkoutCommand(id, userId, request.DurationMinutes, request.Notes, request.IsPrivate));
        return Ok(new { success = true });
    }

    [HttpPost("{id:guid}/pause")]
    public async Task<IActionResult> PauseWorkout(Guid id)
    {
        await _mediator.Send(new PauseWorkoutSessionCommand(id, User.GetRequiredUserId()));
        return NoContent();
    }

    [HttpPost("{id:guid}/resume")]
    public async Task<IActionResult> ResumeWorkout(Guid id)
    {
        await _mediator.Send(new ResumeWorkoutSessionCommand(id, User.GetRequiredUserId()));
        return NoContent();
    }

    [HttpPost("{id:guid}/abandon")]
    public async Task<IActionResult> AbandonWorkout(Guid id)
    {
        await _mediator.Send(new AbandonWorkoutSessionCommand(id, User.GetRequiredUserId()));
        return NoContent();
    }

    [HttpPost("{id:guid}/restore")]
    public async Task<IActionResult> RestoreWorkout(Guid id)
    {
        await _mediator.Send(new RestoreWorkoutCommand(id, User.GetRequiredUserId()));
        return NoContent();
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateWorkout(Guid id, [FromBody] UpdateWorkoutRequest request)
    {
        await _updateWorkoutValidator.ValidateAndThrowAsync(request);

        var userId = User.GetRequiredUserId();
        await _mediator.Send(new UpdateWorkoutCommand(id, userId, request));
        return NoContent();
    }

    [HttpPost("gym/{id:guid}/exercises")]
    public async Task<IActionResult> AddGymExercise(Guid id, [FromBody] AddGymExerciseRequest request,
        [FromServices] IValidator<AddGymExerciseRequest> validator)
    {
        await validator.ValidateAndThrowAsync(request);
        await _mediator.Send(new AddGymExerciseCommand(id, User.GetRequiredUserId(), request));
        return NoContent();
    }

    [HttpPatch("gym/{id:guid}/exercises/{exerciseEntryId:guid}")]
    public async Task<IActionResult> UpdateGymExercise(Guid id, Guid exerciseEntryId, [FromBody] UpdateGymExerciseRequest request,
        [FromServices] IValidator<UpdateGymExerciseRequest> validator)
    {
        await validator.ValidateAndThrowAsync(request);
        await _mediator.Send(new UpdateGymExerciseCommand(id, User.GetRequiredUserId(), exerciseEntryId, request));
        return NoContent();
    }

    [HttpDelete("gym/{id:guid}/exercises/{exerciseEntryId:guid}")]
    public async Task<IActionResult> RemoveGymExercise(Guid id, Guid exerciseEntryId)
    {
        await _mediator.Send(new RemoveGymExerciseCommand(id, User.GetRequiredUserId(), exerciseEntryId));
        return NoContent();
    }

    [HttpPut("gym/{id:guid}/exercises/order")]
    public async Task<IActionResult> ReorderGymExercises(Guid id, [FromBody] ReorderGymItemsRequest request,
        [FromServices] IValidator<ReorderGymItemsRequest> validator)
    {
        await validator.ValidateAndThrowAsync(request);
        await _mediator.Send(new ReorderGymExercisesCommand(id, User.GetRequiredUserId(), request));
        return NoContent();
    }

    [HttpPost("gym/{id:guid}/exercises/{exerciseEntryId:guid}/sets")]
    public async Task<IActionResult> AddGymSet(Guid id, Guid exerciseEntryId, [FromBody] GymSetInputRequest request,
        [FromServices] IValidator<GymSetInputRequest> validator)
    {
        await validator.ValidateAndThrowAsync(request);
        await _mediator.Send(new AddGymSetCommand(id, User.GetRequiredUserId(), exerciseEntryId, request));
        return NoContent();
    }

    [HttpPatch("gym/{id:guid}/exercises/{exerciseEntryId:guid}/sets/{setId:guid}")]
    public async Task<IActionResult> UpdateGymSet(Guid id, Guid exerciseEntryId, Guid setId, [FromBody] UpdateGymSetRequest request,
        [FromServices] IValidator<UpdateGymSetRequest> validator)
    {
        await validator.ValidateAndThrowAsync(request);
        await _mediator.Send(new UpdateGymSetCommand(id, User.GetRequiredUserId(), exerciseEntryId, setId, request));
        return NoContent();
    }

    [HttpPut("gym/{id:guid}/exercises/{exerciseEntryId:guid}/sets/{setId:guid}/completion")]
    public async Task<IActionResult> SetGymSetCompletion(Guid id, Guid exerciseEntryId, Guid setId, [FromBody] SetCompletionRequest request)
    {
        await _mediator.Send(new SetGymSetCompletionCommand(id, User.GetRequiredUserId(), exerciseEntryId, setId, request.IsCompleted));
        return NoContent();
    }

    [HttpDelete("gym/{id:guid}/exercises/{exerciseEntryId:guid}/sets/{setId:guid}")]
    public async Task<IActionResult> RemoveGymSet(Guid id, Guid exerciseEntryId, Guid setId)
    {
        await _mediator.Send(new RemoveGymSetCommand(id, User.GetRequiredUserId(), exerciseEntryId, setId));
        return NoContent();
    }

    [HttpPut("gym/{id:guid}/exercises/{exerciseEntryId:guid}/sets/order")]
    public async Task<IActionResult> ReorderGymSets(Guid id, Guid exerciseEntryId, [FromBody] ReorderGymItemsRequest request,
        [FromServices] IValidator<ReorderGymItemsRequest> validator)
    {
        await validator.ValidateAndThrowAsync(request);
        await _mediator.Send(new ReorderGymSetsCommand(id, User.GetRequiredUserId(), exerciseEntryId, request));
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

    [HttpGet("history")]
    public async Task<ActionResult<WorkoutPageResponse>> GetWorkoutHistory([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        ValidatePagination(page, pageSize);
        return Ok(await _mediator.Send(new GetWorkoutHistoryQuery(User.GetRequiredUserId(), false, page, pageSize)));
    }

    [HttpGet("archived")]
    public async Task<ActionResult<WorkoutPageResponse>> GetArchivedWorkouts([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        ValidatePagination(page, pageSize);
        return Ok(await _mediator.Send(new GetWorkoutHistoryQuery(User.GetRequiredUserId(), true, page, pageSize)));
    }

    [HttpGet("summary")]
    public async Task<ActionResult<ActivitySummaryResponse>> GetActivitySummary([FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        if (from.HasValue && to.HasValue && from > to) return BadRequest("from must be before to.");
        return Ok(await _mediator.Send(new GetActivitySummaryQuery(User.GetRequiredUserId(), from, to)));
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

    [HttpPatch("routines/{routineId:guid}")]
    public async Task<IActionResult> UpdateRoutine(Guid routineId, [FromBody] UpdateRoutineRequest request)
    {
        await _updateRoutineValidator.ValidateAndThrowAsync(request);
        await _mediator.Send(new UpdateWorkoutRoutineCommand(User.GetRequiredUserId(), routineId, request));
        return NoContent();
    }

    private static void ValidatePagination(int page, int pageSize)
    {
        if (page < 1 || pageSize is < 1 or > 100)
            throw new backend.Modules.Shared.Domain.DomainException("Page must be at least one and page size must be between 1 and 100.");
    }
}
