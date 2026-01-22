using backend.Modules.Shared;
using backend.Modules.Workout.Domain.Entities;
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
            request.SplitType
        );

        if (!string.IsNullOrEmpty(request.IntensityLevel))
            workout.SetIntensity(request.IntensityLevel);

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
