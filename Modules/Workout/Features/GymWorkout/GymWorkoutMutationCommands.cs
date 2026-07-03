using backend.Modules.Workout.DTOs;
using backend.Modules.Workout.Services;
using MediatR;

namespace backend.Modules.Workout.Features.GymWorkout;

public record AddGymExerciseCommand(Guid WorkoutId, Guid UserId, AddGymExerciseRequest Request) : IRequest;
public record UpdateGymExerciseCommand(Guid WorkoutId, Guid UserId, Guid ExerciseEntryId, UpdateGymExerciseRequest Request) : IRequest;
public record RemoveGymExerciseCommand(Guid WorkoutId, Guid UserId, Guid ExerciseEntryId) : IRequest;
public record ReorderGymExercisesCommand(Guid WorkoutId, Guid UserId, ReorderGymItemsRequest Request) : IRequest;
public record AddGymSetCommand(Guid WorkoutId, Guid UserId, Guid ExerciseEntryId, GymSetInputRequest Request) : IRequest;
public record UpdateGymSetCommand(Guid WorkoutId, Guid UserId, Guid ExerciseEntryId, Guid SetId, UpdateGymSetRequest Request) : IRequest;
public record SetGymSetCompletionCommand(Guid WorkoutId, Guid UserId, Guid ExerciseEntryId, Guid SetId, bool IsCompleted) : IRequest;
public record RemoveGymSetCommand(Guid WorkoutId, Guid UserId, Guid ExerciseEntryId, Guid SetId) : IRequest;
public record ReorderGymSetsCommand(Guid WorkoutId, Guid UserId, Guid ExerciseEntryId, ReorderGymItemsRequest Request) : IRequest;

public class AddGymExerciseHandler : IRequestHandler<AddGymExerciseCommand>
{
    private readonly IGymWorkoutMutationService _service;
    public AddGymExerciseHandler(IGymWorkoutMutationService service) => _service = service;
    public async Task Handle(AddGymExerciseCommand command, CancellationToken cancellationToken)
    {
        var workout = await _service.GetLiveWorkoutAsync(command.WorkoutId, command.UserId, cancellationToken);
        await _service.EnsureExerciseExistsAsync(command.Request.ExerciseId, cancellationToken);
        var exercise = workout.AddExercise(command.Request.ExerciseId, command.Request.Notes);
        foreach (var set in command.Request.Sets ?? [])
            exercise.AddSet(set.Reps, set.WeightKg, set.DurationSeconds, set.DistanceMeters, set.IsWarmup, set.Rpe, set.Notes, set.IsCompleted);
        await _service.SaveAsync(cancellationToken);
    }
}

public class UpdateGymExerciseHandler : IRequestHandler<UpdateGymExerciseCommand>
{
    private readonly IGymWorkoutMutationService _service;
    public UpdateGymExerciseHandler(IGymWorkoutMutationService service) => _service = service;
    public async Task Handle(UpdateGymExerciseCommand command, CancellationToken cancellationToken)
    {
        var workout = await _service.GetLiveWorkoutAsync(command.WorkoutId, command.UserId, cancellationToken);
        workout.FindExercise(command.ExerciseEntryId).UpdateNotes(command.Request.Notes);
        await _service.SaveAsync(cancellationToken);
    }
}

public class RemoveGymExerciseHandler : IRequestHandler<RemoveGymExerciseCommand>
{
    private readonly IGymWorkoutMutationService _service;
    public RemoveGymExerciseHandler(IGymWorkoutMutationService service) => _service = service;
    public async Task Handle(RemoveGymExerciseCommand command, CancellationToken cancellationToken)
    {
        var workout = await _service.GetLiveWorkoutAsync(command.WorkoutId, command.UserId, cancellationToken);
        workout.RemoveExercise(command.ExerciseEntryId);
        await _service.SaveAsync(cancellationToken);
    }
}

public class ReorderGymExercisesHandler : IRequestHandler<ReorderGymExercisesCommand>
{
    private readonly IGymWorkoutMutationService _service;
    public ReorderGymExercisesHandler(IGymWorkoutMutationService service) => _service = service;
    public async Task Handle(ReorderGymExercisesCommand command, CancellationToken cancellationToken)
    {
        var workout = await _service.GetLiveWorkoutAsync(command.WorkoutId, command.UserId, cancellationToken);
        await _service.ReorderAsync(
            () => workout.Exercises.Select((exercise, index) => (exercise, index)).ToList().ForEach(item => item.exercise.SetOrder(int.MaxValue - item.index)),
            () => workout.ReorderExercises(command.Request.OrderedIds.ToList()), cancellationToken);
    }
}

public class AddGymSetHandler : IRequestHandler<AddGymSetCommand>
{
    private readonly IGymWorkoutMutationService _service;
    public AddGymSetHandler(IGymWorkoutMutationService service) => _service = service;
    public async Task Handle(AddGymSetCommand command, CancellationToken cancellationToken)
    {
        var workout = await _service.GetLiveWorkoutAsync(command.WorkoutId, command.UserId, cancellationToken);
        var set = command.Request;
        workout.FindExercise(command.ExerciseEntryId).AddSet(set.Reps, set.WeightKg, set.DurationSeconds, set.DistanceMeters, set.IsWarmup, set.Rpe, set.Notes, set.IsCompleted);
        await _service.SaveAsync(cancellationToken);
    }
}

public class UpdateGymSetHandler : IRequestHandler<UpdateGymSetCommand>
{
    private readonly IGymWorkoutMutationService _service;
    public UpdateGymSetHandler(IGymWorkoutMutationService service) => _service = service;
    public async Task Handle(UpdateGymSetCommand command, CancellationToken cancellationToken)
    {
        var workout = await _service.GetLiveWorkoutAsync(command.WorkoutId, command.UserId, cancellationToken);
        var set = command.Request;
        workout.FindExercise(command.ExerciseEntryId).UpdateSet(command.SetId, set.Reps, set.WeightKg, set.DurationSeconds, set.DistanceMeters, set.IsWarmup, set.Rpe, set.Notes);
        await _service.SaveAsync(cancellationToken);
    }
}

public class SetGymSetCompletionHandler : IRequestHandler<SetGymSetCompletionCommand>
{
    private readonly IGymWorkoutMutationService _service;
    public SetGymSetCompletionHandler(IGymWorkoutMutationService service) => _service = service;
    public async Task Handle(SetGymSetCompletionCommand command, CancellationToken cancellationToken)
    {
        var workout = await _service.GetLiveWorkoutAsync(command.WorkoutId, command.UserId, cancellationToken);
        workout.FindExercise(command.ExerciseEntryId).SetSetCompletion(command.SetId, command.IsCompleted, DateTime.UtcNow);
        await _service.SaveAsync(cancellationToken);
    }
}

public class RemoveGymSetHandler : IRequestHandler<RemoveGymSetCommand>
{
    private readonly IGymWorkoutMutationService _service;
    public RemoveGymSetHandler(IGymWorkoutMutationService service) => _service = service;
    public async Task Handle(RemoveGymSetCommand command, CancellationToken cancellationToken)
    {
        var workout = await _service.GetLiveWorkoutAsync(command.WorkoutId, command.UserId, cancellationToken);
        workout.FindExercise(command.ExerciseEntryId).RemoveSet(command.SetId);
        await _service.SaveAsync(cancellationToken);
    }
}

public class ReorderGymSetsHandler : IRequestHandler<ReorderGymSetsCommand>
{
    private readonly IGymWorkoutMutationService _service;
    public ReorderGymSetsHandler(IGymWorkoutMutationService service) => _service = service;
    public async Task Handle(ReorderGymSetsCommand command, CancellationToken cancellationToken)
    {
        var workout = await _service.GetLiveWorkoutAsync(command.WorkoutId, command.UserId, cancellationToken);
        var exercise = workout.FindExercise(command.ExerciseEntryId);
        await _service.ReorderAsync(
            () => exercise.WorkoutSets.Select((set, index) => (set, index)).ToList().ForEach(item => item.set.SetOrder(int.MaxValue - item.index)),
            () => exercise.ReorderSets(command.Request.OrderedIds), cancellationToken);
    }
}
