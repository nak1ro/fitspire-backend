using backend.Modules.Shared.Domain;
using backend.Modules.Workout.Infrastructure;

namespace backend.Modules.Workout.Services;

public interface IWorkoutSessionGuard
{
    Task EnsureCanStartAsync(Guid userId, CancellationToken cancellationToken = default);
}

public class WorkoutSessionGuard : IWorkoutSessionGuard
{
    private readonly IWorkoutRepository _repository;

    public WorkoutSessionGuard(IWorkoutRepository repository)
    {
        _repository = repository;
    }

    public async Task EnsureCanStartAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        if (await _repository.GetActiveSessionByUserIdAsync(userId, cancellationToken) is not null)
            throw new ConflictException("Finish or abandon the current active workout before starting another session.");
    }
}
