using backend.Modules.Shared;
using backend.Modules.Workout.Domain.Entities;
using backend.Modules.Workout.Domain.Enums;
using backend.Modules.Workout.Infrastructure;
using MediatR;

namespace backend.Modules.Workout.Features.GymWorkout;

public class CreateGymWorkoutHandler : IRequestHandler<CreateGymWorkoutCommand, Guid>
{
    private readonly IWorkoutRepository _workoutRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateGymWorkoutHandler(IWorkoutRepository workoutRepository, IUnitOfWork unitOfWork)
    {
        _workoutRepository = workoutRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateGymWorkoutCommand request, CancellationToken cancellationToken)
    {
        var workout = new GymUserWorkoutDetails(
            Guid.NewGuid(),
            request.UserId,
            request.Date,
            Enum.TryParse<WorkoutSplit>(request.SplitType, true, out var split) ? split : null
        );

        if (Enum.TryParse<WorkoutIntensity>(request.IntensityLevel, true, out var intensity))
            workout.SetIntensity(intensity);

        foreach (var exercise in request.Exercises)
        {
            workout.AddExercise(
                exercise.ExerciseId,
                exercise.Sets,
                exercise.Reps,
                exercise.WeightKg
            );
        }

        await _workoutRepository.AddAsync(workout, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return workout.Id;
    }
}
