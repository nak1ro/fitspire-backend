using AutoMapper;
using backend.Modules.Workout.DTOs;
using backend.Modules.Workout.Infrastructure;
using MediatR;

namespace backend.Modules.Workout.Features.Common;

public record GetExerciseCategoriesQuery : IRequest<List<ExerciseCategoryResponse>>;

public class GetExerciseCategoriesHandler : IRequestHandler<GetExerciseCategoriesQuery, List<ExerciseCategoryResponse>>
{
    private readonly IWorkoutRepository _workoutRepository;
    private readonly IMapper _mapper;

    public GetExerciseCategoriesHandler(IWorkoutRepository workoutRepository, IMapper mapper)
    {
        _workoutRepository = workoutRepository;
        _mapper = mapper;
    }

    public async Task<List<ExerciseCategoryResponse>> Handle(GetExerciseCategoriesQuery request, CancellationToken cancellationToken)
    {
        var categories = await _workoutRepository.GetExerciseCategoriesAsync(cancellationToken);
        return _mapper.Map<List<ExerciseCategoryResponse>>(categories);
    }
}
