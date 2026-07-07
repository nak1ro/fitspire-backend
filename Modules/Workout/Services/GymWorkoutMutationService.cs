using backend.Data;
using backend.Modules.Shared;
using backend.Modules.Shared.Domain;
using backend.Modules.Workout.Domain.Entities;
using backend.Modules.Workout.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace backend.Modules.Workout.Services;

public interface IGymWorkoutMutationService
{
    Task<GymUserWorkoutDetails> GetLiveWorkoutAsync(Guid workoutId, Guid userId, CancellationToken cancellationToken);
    Task EnsureExerciseExistsAsync(Guid exerciseId, CancellationToken cancellationToken);
    Task SaveAsync(CancellationToken cancellationToken);
    Task ReorderAsync(Action stageTemporaryOrder, Action applyFinalOrder, CancellationToken cancellationToken);
}

public class GymWorkoutMutationService : IGymWorkoutMutationService
{
    private readonly IWorkoutRepository _workoutRepository;
    private readonly FitspireDbContext _context;
    private readonly IUnitOfWork _unitOfWork;

    public GymWorkoutMutationService(IWorkoutRepository workoutRepository, FitspireDbContext context, IUnitOfWork unitOfWork)
    {
        _workoutRepository = workoutRepository;
        _context = context;
        _unitOfWork = unitOfWork;
    }

    public async Task<GymUserWorkoutDetails> GetLiveWorkoutAsync(Guid workoutId, Guid userId, CancellationToken cancellationToken)
    {
        var workout = await _workoutRepository.GetGymWorkoutByIdAsync(workoutId, cancellationToken)
            ?? throw new NotFoundException($"Gym workout {workoutId} not found.");

        if (workout.UserId != userId)
            throw new UnauthorizedAccessException("Cannot modify another user's workout.");

        workout.EnsureLiveSessionMutationAllowed();
        return workout;
    }

    public async Task EnsureExerciseExistsAsync(Guid exerciseId, CancellationToken cancellationToken)
    {
        if (!await _context.Exercises.AnyAsync(exercise => exercise.Id == exerciseId, cancellationToken))
            throw new NotFoundException($"Exercise {exerciseId} not found.");
    }

    public Task SaveAsync(CancellationToken cancellationToken) => _unitOfWork.SaveChangesAsync(cancellationToken);

    public async Task ReorderAsync(Action stageTemporaryOrder, Action applyFinalOrder, CancellationToken cancellationToken)
    {
        var ownsTransaction = _context.Database.CurrentTransaction is null;
        await using var transaction = ownsTransaction
            ? await _context.Database.BeginTransactionAsync(cancellationToken)
            : null;
        stageTemporaryOrder();
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        applyFinalOrder();
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        if (transaction is not null)
            await transaction.CommitAsync(cancellationToken);
    }
}
