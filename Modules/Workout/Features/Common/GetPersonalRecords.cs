using AutoMapper;
using backend.Modules.Workout.DTOs;
using backend.Modules.Workout.Infrastructure;
using MediatR;

namespace backend.Modules.Workout.Features.Common;

public record GetPersonalRecordsQuery(Guid UserId) : IRequest<List<PersonalRecordResponse>>;

public class GetPersonalRecordsHandler : IRequestHandler<GetPersonalRecordsQuery, List<PersonalRecordResponse>>
{
    private readonly IWorkoutRepository _workoutRepository;
    private readonly IMapper _mapper;

    public GetPersonalRecordsHandler(IWorkoutRepository workoutRepository, IMapper mapper)
    {
        _workoutRepository = workoutRepository;
        _mapper = mapper;
    }

    public async Task<List<PersonalRecordResponse>> Handle(
        GetPersonalRecordsQuery request,
        CancellationToken cancellationToken)
    {
        var records = await _workoutRepository.GetPersonalRecordsByUserIdAsync(request.UserId, cancellationToken);
        return _mapper.Map<List<PersonalRecordResponse>>(records);
    }
}
