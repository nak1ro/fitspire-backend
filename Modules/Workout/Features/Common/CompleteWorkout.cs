using backend.Modules.Shared;
using backend.Modules.Shared.Domain;
using backend.Modules.Workout.Infrastructure;
using MediatR;

namespace backend.Modules.Workout.Features.Common;

public record CompleteWorkoutCommand(
    Guid WorkoutId,
    double? DurationMinutes
) : IRequest<bool>;

public class CompleteWorkoutHandler : IRequestHandler<CompleteWorkoutCommand, bool>
{
    private readonly IWorkoutRepository _workoutRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CompleteWorkoutHandler(IWorkoutRepository workoutRepository, IUnitOfWork unitOfWork)
    {
        _workoutRepository = workoutRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(CompleteWorkoutCommand request, CancellationToken cancellationToken)
    {
        var workout = await _workoutRepository.GetByIdAsync(request.WorkoutId, cancellationToken);
        
        if (workout is null)
            throw new DomainException($"Workout {request.WorkoutId} not found.");

        workout.Complete(request.DurationMinutes);

        await _workoutRepository.UpdateAsync(workout, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
