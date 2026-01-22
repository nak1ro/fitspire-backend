using AutoMapper;
using backend.Modules.Workout.DTOs;
using backend.Modules.Workout.Infrastructure;
using MediatR;

namespace backend.Modules.Workout.Features.Common;

public class GetWorkoutsHandler : IRequestHandler<GetWorkoutsQuery, List<WorkoutResponse>>
{
    private readonly IWorkoutRepository _repository;
    private readonly IMapper _mapper;

    public GetWorkoutsHandler(IWorkoutRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<List<WorkoutResponse>> Handle(GetWorkoutsQuery request, CancellationToken cancellationToken)
    {
        var workouts = await _repository.SearchAsync(
            request.UserId,
            request.Filter.From,
            request.Filter.To,
            request.Filter.Types,
            cancellationToken
        );

        return _mapper.Map<List<WorkoutResponse>>(workouts);
    }
}
