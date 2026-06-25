using System.Text.Json;
using backend.Modules.Shared;
using backend.Modules.Shared.Domain;
using backend.Modules.Workout.DTOs;
using backend.Modules.Workout.Infrastructure;
using MediatR;

namespace backend.Modules.Workout.Features.Common;

public record UpdateWorkoutRoutineCommand(Guid UserId, Guid RoutineId, UpdateRoutineRequest Request) : IRequest;

public class UpdateWorkoutRoutineHandler : IRequestHandler<UpdateWorkoutRoutineCommand>
{
    private readonly IWorkoutRepository _repository; private readonly IUnitOfWork _unitOfWork;
    public UpdateWorkoutRoutineHandler(IWorkoutRepository repository, IUnitOfWork unitOfWork) { _repository = repository; _unitOfWork = unitOfWork; }
    public async Task Handle(UpdateWorkoutRoutineCommand request, CancellationToken cancellationToken)
    {
        var routine = await _repository.GetRoutineByIdAsync(request.RoutineId, cancellationToken) ?? throw new NotFoundException("Routine not found.");
        if (routine.UserId != request.UserId) throw new UnauthorizedAccessException("Cannot edit another user's routine.");
        var definition = request.Request.Definition;
        if (!definition.TryGetProperty("SchemaVersion", out var version) || version.GetInt32() != 1)
            throw new DomainException("Routine definition must use schema version 1.");
        if (!definition.TryGetProperty("WorkoutType", out var type) || !string.Equals(type.GetString(), routine.WorkoutType, StringComparison.OrdinalIgnoreCase))
            throw new DomainException("Routine definition workout type must match the routine.");
        routine.UpdateDetails(request.Request.Name.Trim(), request.Request.Description?.Trim(), JsonSerializer.Serialize(definition));
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
