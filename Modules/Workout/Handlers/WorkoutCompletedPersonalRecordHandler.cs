using backend.Modules.Shared;
using backend.Modules.Workout.Domain.Entities;
using backend.Modules.Workout.Domain.Events;
using backend.Modules.Workout.Infrastructure;
using MediatR;

namespace backend.Modules.Workout.Handlers;

public class WorkoutCompletedPersonalRecordHandler : INotificationHandler<WorkoutCompletedEvent>
{
    private readonly IWorkoutRepository _workoutRepository;
    private readonly IUnitOfWork _unitOfWork;

    public WorkoutCompletedPersonalRecordHandler(IWorkoutRepository workoutRepository, IUnitOfWork unitOfWork)
    {
        _workoutRepository = workoutRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(WorkoutCompletedEvent notification, CancellationToken cancellationToken)
    {
        var candidates = await BuildRecordCandidatesAsync(notification, cancellationToken);

        foreach (var candidate in candidates.Where(candidate => candidate.Value > 0))
        {
            await ApplyCandidateAsync(notification, candidate, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<List<RecordCandidate>> BuildRecordCandidatesAsync(
        WorkoutCompletedEvent notification,
        CancellationToken cancellationToken)
    {
        var records = new List<RecordCandidate>();

        AddCommonRecords(records, notification);

        if (notification.WorkoutType == "gym")
        {
            var gymWorkout = await _workoutRepository.GetGymWorkoutByIdAsync(notification.WorkoutId, cancellationToken);
            if (gymWorkout is not null)
            {
                records.Add(new RecordCandidate("max_weight", gymWorkout.GetMaxWeight() ?? 0));
                records.Add(new RecordCandidate("total_volume", gymWorkout.CalculateTotalVolume()));
            }
        }
        else if (notification.DistanceKm.HasValue)
        {
            records.Add(new RecordCandidate("distance", notification.DistanceKm.Value));
        }

        return records;
    }

    private static void AddCommonRecords(List<RecordCandidate> records, WorkoutCompletedEvent notification)
    {
        if (notification.DurationMinutes.HasValue)
            records.Add(new RecordCandidate("duration", notification.DurationMinutes.Value));

        if (notification.CaloriesBurned.HasValue)
            records.Add(new RecordCandidate("calories", notification.CaloriesBurned.Value));
    }

    private async Task ApplyCandidateAsync(
        WorkoutCompletedEvent notification,
        RecordCandidate candidate,
        CancellationToken cancellationToken)
    {
        var record = await _workoutRepository.GetPersonalRecordAsync(
            notification.UserId,
            notification.WorkoutType,
            candidate.Metric,
            cancellationToken);

        if (record is null)
        {
            record = PersonalRecord.Create(
                notification.UserId,
                notification.WorkoutType,
                candidate.Metric,
                candidate.Value,
                notification.WorkoutId);

            await _workoutRepository.AddPersonalRecordAsync(record, cancellationToken);
            await AddHistoryAsync(record, cancellationToken);
            return;
        }

        if (!record.TryBeat(candidate.Value, notification.WorkoutId))
            return;

        await _workoutRepository.UpdatePersonalRecordAsync(record, cancellationToken);
        await AddHistoryAsync(record, cancellationToken);
    }

    private async Task AddHistoryAsync(PersonalRecord record, CancellationToken cancellationToken)
    {
        var history = new PersonalRecordHistory
        {
            Id = Guid.NewGuid(),
            UserId = record.UserId,
            WorkoutType = record.WorkoutType,
            Metric = record.Metric,
            Value = record.Value,
            WorkoutId = record.WorkoutId,
            RecordedAt = DateTime.UtcNow
        };

        await _workoutRepository.AddPersonalRecordHistoryAsync(history, cancellationToken);
    }

    private record RecordCandidate(string Metric, double Value);
}
