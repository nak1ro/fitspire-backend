using AutoMapper;
using backend.Modules.Shared.Domain;
using backend.Modules.Workout.DTOs;
using backend.Modules.Workout.Infrastructure;
using MediatR;

namespace backend.Modules.Workout.Features.Common;

public record GetWorkoutRoutineByIdQuery(Guid UserId, Guid RoutineId) : IRequest<WorkoutRoutineResponse>;

public class GetWorkoutRoutineByIdHandler : IRequestHandler<GetWorkoutRoutineByIdQuery, WorkoutRoutineResponse>
{
    private readonly IWorkoutRepository _workoutRepository;
    private readonly IMapper _mapper;

    public GetWorkoutRoutineByIdHandler(IWorkoutRepository workoutRepository, IMapper mapper)
    {
        _workoutRepository = workoutRepository;
        _mapper = mapper;
    }

    public async Task<WorkoutRoutineResponse> Handle(GetWorkoutRoutineByIdQuery request, CancellationToken cancellationToken)
    {
        var routine = await _workoutRepository.GetRoutineByIdAsync(request.RoutineId, cancellationToken);

        if (routine == null)
            throw new NotFoundException($"Routine {request.RoutineId} not found.");

        if (routine.UserId != request.UserId)
            throw new UnauthorizedAccessException("Cannot view another user's routine.");

        return _mapper.Map<WorkoutRoutineResponse>(routine);
    }
}
