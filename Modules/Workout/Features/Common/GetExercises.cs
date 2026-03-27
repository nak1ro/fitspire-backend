using AutoMapper;
using backend.Modules.Workout.DTOs;
using backend.Modules.Workout.Infrastructure;
using MediatR;

namespace backend.Modules.Workout.Features.Common;

public record GetExercisesQuery(Guid? CategoryId, string? Search) : IRequest<List<ExerciseResponse>>;

public class GetExercisesHandler : IRequestHandler<GetExercisesQuery, List<ExerciseResponse>>
{
    private readonly IWorkoutRepository _workoutRepository;
    private readonly IMapper _mapper;

    public GetExercisesHandler(IWorkoutRepository workoutRepository, IMapper mapper)
    {
        _workoutRepository = workoutRepository;
        _mapper = mapper;
    }

    public async Task<List<ExerciseResponse>> Handle(GetExercisesQuery request, CancellationToken cancellationToken)
    {
        var exercises = await _workoutRepository.GetExercisesAsync(
            request.CategoryId,
            request.Search,
            cancellationToken);

        return _mapper.Map<List<ExerciseResponse>>(exercises);
    }
}
