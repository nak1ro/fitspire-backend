using backend.Modules.Shared;
using backend.Modules.Workout.Domain.Entities;
using backend.Modules.Workout.Infrastructure;
using MediatR;

namespace backend.Modules.Workout.Features.CyclingWorkout;

public record CreateCyclingWorkoutCommand(
    Guid UserId,
    DateTime Date,
    double DistanceKm,
    double? DurationMinutes,
    double? ElevationGainMeters,
    int? CaloriesBurned,
    string? MapData,
    string? Notes,
    bool IsPrivate,
    bool IsIndoor
) : IRequest<Guid>;

public class CreateCyclingWorkoutHandler : IRequestHandler<CreateCyclingWorkoutCommand, Guid>
{
    private readonly IWorkoutRepository _workoutRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateCyclingWorkoutHandler(IWorkoutRepository workoutRepository, IUnitOfWork unitOfWork)
    {
        _workoutRepository = workoutRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateCyclingWorkoutCommand request, CancellationToken cancellationToken)
    {
        var workout = new CyclingUserWorkoutDetails(
            Guid.NewGuid(),
            request.UserId,
            request.Date,
            request.DistanceKm,
            request.DurationMinutes,
            request.IsIndoor
        );

        workout.UpdateStats(request.ElevationGainMeters, request.MapData);
        workout.SetCalories(request.CaloriesBurned);
        
        if (!string.IsNullOrEmpty(request.Notes))
            workout.UpdateNotes(request.Notes);
            
        if (request.IsPrivate)
            workout.SetPrivacy(true);

        await _workoutRepository.AddAsync(workout, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return workout.Id;
    }
}
