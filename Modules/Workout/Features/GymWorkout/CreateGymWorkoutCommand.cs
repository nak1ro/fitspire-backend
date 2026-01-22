using MediatR;

namespace backend.Modules.Workout.Features.GymWorkout;

public record CreateGymWorkoutCommand(
    Guid UserId,
    DateTime Date,
    string? SplitType,
    string? IntensityLevel,
    List<ExerciseInput> Exercises
) : IRequest<Guid>;

public record ExerciseInput(
    Guid ExerciseId,
    int Sets,
    int Reps,
    double WeightKg
);
