using backend.Modules.Shared;
using backend.Modules.Workout.Domain.Entities;
using backend.Modules.Workout.Domain.Enums;
using backend.Modules.Workout.Infrastructure;
using backend.Modules.Workout.Services;
using MediatR;

namespace backend.Modules.Workout.Features.SwimmingWorkout;

public record CreateSwimmingWorkoutCommand(
    Guid UserId,
    DateTime Date,
    int? Laps,
    double? PoolLengthMeters,
    double? DistanceMeters,
    string? StrokeType,
    double? DurationMinutes,
    int? CaloriesBurned,
    string? Notes,
    bool IsPrivate
) : IRequest<Guid>;

public class CreateSwimmingWorkoutHandler : IRequestHandler<CreateSwimmingWorkoutCommand, Guid>
{
    private readonly IWorkoutRepository _workoutRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IWorkoutDerivedDataService _derivedData;

    public CreateSwimmingWorkoutHandler(IWorkoutRepository workoutRepository, IUnitOfWork unitOfWork, IWorkoutDerivedDataService derivedData)
    {
        _workoutRepository = workoutRepository;
        _unitOfWork = unitOfWork;
        _derivedData = derivedData;
    }

    public async Task<Guid> Handle(CreateSwimmingWorkoutCommand request, CancellationToken cancellationToken)
    {
        var workout = new SwimmingUserWorkoutDetails(
            Guid.NewGuid(),
            request.UserId,
            request.Date,
            null
        );

        workout.SetPoolDetails(request.Laps, request.PoolLengthMeters);
        
        // If distance is manually provided, it overrides calculation
        if (request.DistanceMeters.HasValue)
            workout.SetDistance(request.DistanceMeters);
            
        if (Enum.TryParse<SwimmingStroke>(request.StrokeType, true, out var stroke))
            workout.SetStrokeType(stroke);
        workout.SetCalories(request.CaloriesBurned);
        
        if (!string.IsNullOrEmpty(request.Notes))
            workout.UpdateNotes(request.Notes);
            
        if (request.IsPrivate)
            workout.SetPrivacy(true);

        if (request.DurationMinutes.HasValue)
            workout.Complete(request.DurationMinutes);

        await _workoutRepository.AddAsync(workout, cancellationToken);
        if (workout.Status == Domain.Enums.WorkoutStatus.Completed)
            await _derivedData.ReconcileCompletedWorkoutAsync(workout, cancellationToken);
        else
            await _unitOfWork.SaveChangesAsync(cancellationToken);

        return workout.Id;
    }
}
