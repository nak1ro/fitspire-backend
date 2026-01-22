using System.Text.Json;
using backend.Modules.Shared;
using backend.Modules.Shared.Domain;
using backend.Modules.Workout.Domain.Entities;
using backend.Modules.Workout.Infrastructure;
using MediatR;

namespace backend.Modules.Workout.Features.Common;

public record CreateWorkoutFromRoutineCommand(
    Guid CurrentUserId,
    Guid RoutineId,
    DateTime Date
) : IRequest<Guid>;

public class CreateWorkoutFromRoutineHandler : IRequestHandler<CreateWorkoutFromRoutineCommand, Guid>
{
    private readonly IWorkoutRepository _workoutRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateWorkoutFromRoutineHandler(IWorkoutRepository workoutRepository, IUnitOfWork unitOfWork)
    {
        _workoutRepository = workoutRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateWorkoutFromRoutineCommand request, CancellationToken cancellationToken)
    {
        var routine = await _workoutRepository.GetRoutineByIdAsync(request.RoutineId, cancellationToken);
        if (routine == null)
            throw new NotFoundException($"Routine {request.RoutineId} not found.");

        UserWorkout newWorkout;
        var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        // Simple factory based on type (could be improved with a proper Factory pattern)
        if (routine.WorkoutType == "gym")
        {
            var template = JsonSerializer.Deserialize<GymUserWorkoutDetails>(routine.RoutineDataJson, jsonOptions);
            if (template == null) throw new Exception("Invalid routine data.");

            var gymWorkout = new GymUserWorkoutDetails(
                Guid.NewGuid(),
                request.CurrentUserId,
                request.Date,
                template.SplitType
            );
            
            // Copy exercises
            foreach (var ex in template.Exercises)
            {
                gymWorkout.AddExercise(ex.ExerciseId, ex.Sets, ex.Reps, ex.Weight);
            }
            newWorkout = gymWorkout;
        }
        else if (routine.WorkoutType == "running")
        {
             var template = JsonSerializer.Deserialize<RunningUserWorkoutDetails>(routine.RoutineDataJson, jsonOptions);
             if (template == null) throw new Exception("Invalid routine data.");
             
             newWorkout = new RunningUserWorkoutDetails(
                 Guid.NewGuid(),
                 request.CurrentUserId,
                 request.Date,
                 template.DistanceKm,
                 template.DurationMinutes
             );
        }
        // ... Add other types as needed
        else
        {
            throw new Exception($"Unsupported routine type: {routine.WorkoutType}");
        }

        newWorkout.SetCreatedFromRoutine(routine.Id);

        await _workoutRepository.AddAsync(newWorkout, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return newWorkout.Id;
    }
}
