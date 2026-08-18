using backend.Modules.Shared;
using backend.Modules.Shared.Domain;
using backend.Modules.Workout.Infrastructure;
using MediatR;

namespace backend.Modules.Workout.Features.Common;

public record SetFeaturedPersonalRecordCommand(Guid UserId, Guid? PersonalRecordId) : IRequest;

public class SetFeaturedPersonalRecordHandler : IRequestHandler<SetFeaturedPersonalRecordCommand>
{
    private readonly IWorkoutRepository _workoutRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SetFeaturedPersonalRecordHandler(IWorkoutRepository workoutRepository, IUnitOfWork unitOfWork)
    {
        _workoutRepository = workoutRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(SetFeaturedPersonalRecordCommand request, CancellationToken cancellationToken)
    {
        var records = await _workoutRepository.GetPersonalRecordsByUserIdAsync(request.UserId, cancellationToken);

        var target = request.PersonalRecordId.HasValue
            ? records.FirstOrDefault(record => record.Id == request.PersonalRecordId.Value)
            : null;
        if (request.PersonalRecordId.HasValue && target is null)
            throw new NotFoundException($"Personal record {request.PersonalRecordId} not found.");

        var currentlyFeatured = records.FirstOrDefault(record => record.IsFeatured);
        if (currentlyFeatured is not null && currentlyFeatured != target)
        {
            currentlyFeatured.ClearFeatured();
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        if (target is not null && !target.IsFeatured)
        {
            target.SetFeatured();
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
