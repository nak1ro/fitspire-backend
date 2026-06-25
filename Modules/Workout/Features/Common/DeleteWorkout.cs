using backend.Modules.Shared.Domain;
using backend.Modules.Workout.Infrastructure;
using backend.Modules.Workout.Services;
using MediatR;

namespace backend.Modules.Workout.Features.Common;

public record DeleteWorkoutCommand(Guid WorkoutId, Guid UserId) : IRequest;

public class DeleteWorkoutHandler : IRequestHandler<DeleteWorkoutCommand>
{
    private readonly IWorkoutRepository _repository;
    private readonly IWorkoutDerivedDataService _derivedData;

    public DeleteWorkoutHandler(IWorkoutRepository repository, IWorkoutDerivedDataService derivedData)
    {
        _repository = repository;
        _derivedData = derivedData;
    }

    public async Task Handle(DeleteWorkoutCommand request, CancellationToken cancellationToken)
    {
        var workout = await _repository.GetByIdAsync(request.WorkoutId, cancellationToken);

        if (workout == null)
            throw new NotFoundException($"Workout {request.WorkoutId} not found.");

        if (workout.UserId != request.UserId)
            throw new UnauthorizedAccessException("Cannot delete another user's workout.");

        workout.Delete();
        await _derivedData.ReconcileDeletedWorkoutAsync(workout.UserId, workout.Id, cancellationToken);
    }
}
