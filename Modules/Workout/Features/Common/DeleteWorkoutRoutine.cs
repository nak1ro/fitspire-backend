using backend.Modules.Shared;
using backend.Modules.Shared.Domain;
using backend.Modules.Workout.Infrastructure;
using MediatR;

namespace backend.Modules.Workout.Features.Common;

public record DeleteWorkoutRoutineCommand(Guid UserId, Guid RoutineId) : IRequest;

public class DeleteWorkoutRoutineHandler : IRequestHandler<DeleteWorkoutRoutineCommand>
{
    private readonly IWorkoutRepository _workoutRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteWorkoutRoutineHandler(IWorkoutRepository workoutRepository, IUnitOfWork unitOfWork)
    {
        _workoutRepository = workoutRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeleteWorkoutRoutineCommand request, CancellationToken cancellationToken)
    {
        var routine = await _workoutRepository.GetRoutineByIdAsync(request.RoutineId, cancellationToken);

        if (routine == null)
            throw new NotFoundException($"Routine {request.RoutineId} not found.");

        if (routine.UserId != request.UserId)
            throw new UnauthorizedAccessException("Cannot delete another user's routine.");

        await _workoutRepository.DeleteRoutineAsync(routine, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
