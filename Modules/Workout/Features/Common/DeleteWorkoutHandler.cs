using backend.Modules.Shared;
using backend.Modules.Shared.Domain;
using backend.Modules.Workout.Infrastructure;
using MediatR;

namespace backend.Modules.Workout.Features.Common;

public class DeleteWorkoutHandler : IRequestHandler<DeleteWorkoutCommand>
{
    private readonly IWorkoutRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteWorkoutHandler(IWorkoutRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeleteWorkoutCommand request, CancellationToken cancellationToken)
    {
        var workout = await _repository.GetByIdAsync(request.WorkoutId, cancellationToken);

        if (workout == null)
            throw new NotFoundException($"Workout {request.WorkoutId} not found.");

        if (workout.UserId != request.UserId)
            throw new UnauthorizedAccessException("Cannot delete another user's workout.");

        await _repository.DeleteAsync(workout, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
