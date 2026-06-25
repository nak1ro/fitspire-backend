using backend.Modules.Shared;
using backend.Modules.Shared.Domain;
using backend.Modules.Workout.Domain.Entities;
using backend.Modules.Workout.DTOs;
using backend.Modules.Workout.Infrastructure;
using backend.Modules.Progress.Services;
using MediatR;

namespace backend.Modules.Workout.Features.Sessions;

public record PauseWorkoutSessionCommand(Guid WorkoutId, Guid UserId) : IRequest;
public record ResumeWorkoutSessionCommand(Guid WorkoutId, Guid UserId) : IRequest;
public record AbandonWorkoutSessionCommand(Guid WorkoutId, Guid UserId) : IRequest;
public record RestoreWorkoutCommand(Guid WorkoutId, Guid UserId) : IRequest;
public record GetActiveWorkoutSessionQuery(Guid UserId) : IRequest<WorkoutSessionResponse?>;

public class PauseWorkoutSessionHandler : IRequestHandler<PauseWorkoutSessionCommand>
{
    private readonly IWorkoutRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public PauseWorkoutSessionHandler(IWorkoutRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(PauseWorkoutSessionCommand request, CancellationToken cancellationToken)
    {
        var workout = await WorkoutSessionAccess.GetOwnedWorkoutAsync(_repository, request.WorkoutId, request.UserId, cancellationToken);
        workout.Pause(DateTime.UtcNow);
        await _repository.UpdateAsync(workout, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

public class ResumeWorkoutSessionHandler : IRequestHandler<ResumeWorkoutSessionCommand>
{
    private readonly IWorkoutRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public ResumeWorkoutSessionHandler(IWorkoutRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(ResumeWorkoutSessionCommand request, CancellationToken cancellationToken)
    {
        var workout = await WorkoutSessionAccess.GetOwnedWorkoutAsync(_repository, request.WorkoutId, request.UserId, cancellationToken);
        workout.Resume(DateTime.UtcNow);
        await _repository.UpdateAsync(workout, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

public class AbandonWorkoutSessionHandler : IRequestHandler<AbandonWorkoutSessionCommand>
{
    private readonly IWorkoutRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public AbandonWorkoutSessionHandler(IWorkoutRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(AbandonWorkoutSessionCommand request, CancellationToken cancellationToken)
    {
        var workout = await WorkoutSessionAccess.GetOwnedWorkoutAsync(_repository, request.WorkoutId, request.UserId, cancellationToken);
        workout.Abandon();
        await _repository.UpdateAsync(workout, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

public class RestoreWorkoutHandler : IRequestHandler<RestoreWorkoutCommand>
{
    private readonly IWorkoutRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IContributionReconciliationService _contributions;

    public RestoreWorkoutHandler(IWorkoutRepository repository, IUnitOfWork unitOfWork, IContributionReconciliationService contributions)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _contributions = contributions;
    }

    public async Task Handle(RestoreWorkoutCommand request, CancellationToken cancellationToken)
    {
        var workout = await _repository.GetArchivedByIdAsync(request.WorkoutId, cancellationToken)
            ?? throw new NotFoundException($"Archived workout {request.WorkoutId} not found.");

        if (workout.UserId != request.UserId)
            throw new UnauthorizedAccessException("Cannot restore another user's workout.");

        workout.Restore();
        await _repository.UpdateAsync(workout, cancellationToken);
        if (workout.Status == Domain.Enums.WorkoutStatus.Completed)
            await _contributions.ReconcileWorkoutAsync(workout, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

public class GetActiveWorkoutSessionHandler : IRequestHandler<GetActiveWorkoutSessionQuery, WorkoutSessionResponse?>
{
    private readonly IWorkoutRepository _repository;

    public GetActiveWorkoutSessionHandler(IWorkoutRepository repository)
    {
        _repository = repository;
    }

    public async Task<WorkoutSessionResponse?> Handle(GetActiveWorkoutSessionQuery request, CancellationToken cancellationToken)
    {
        var workout = await _repository.GetActiveSessionByUserIdAsync(request.UserId, cancellationToken);
        return workout is null ? null : WorkoutSessionMapper.MapSession(workout, DateTime.UtcNow);
    }
}

internal static class WorkoutSessionMapper
{
    public static WorkoutSessionResponse MapSession(UserWorkout workout, DateTime nowUtc)
    {
        var end = workout.PausedAt ?? nowUtc;
        var elapsedMinutes = workout.StartedAt is null
            ? 0
            : Math.Max(0, (end - workout.StartedAt.Value).TotalMinutes - workout.AccumulatedPausedSeconds / 60d);

        return new WorkoutSessionResponse(
            workout.Id,
            workout.WorkoutType,
            workout.Status.ToString(),
            workout.StartedAt,
            workout.PausedAt,
            workout.AccumulatedPausedSeconds,
            elapsedMinutes);
    }
}

internal static class WorkoutSessionAccess
{
    public static async Task<UserWorkout> GetOwnedWorkoutAsync(
        IWorkoutRepository repository,
        Guid workoutId,
        Guid userId,
        CancellationToken cancellationToken)
    {
        var workout = await repository.GetByIdAsync(workoutId, cancellationToken)
            ?? throw new NotFoundException($"Workout {workoutId} not found.");

        if (workout.UserId != userId)
            throw new UnauthorizedAccessException("Workout does not belong to the current user.");

        return workout;
    }
}
