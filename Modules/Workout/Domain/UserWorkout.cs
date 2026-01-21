using backend.Modules.Shared.Domain;
using backend.Modules.User.Domain;
using backend.Modules.Workout.Domain.Events;

namespace backend.Modules.Workout.Domain;

public abstract class UserWorkout : AggregateRoot<Guid>
{
    public Guid UserId { get; private set; }
    public string WorkoutType { get; private set; } = null!;
    public DateTime Date { get; private set; }
    public double? DurationMinutes { get; private set; }
    public string? Notes { get; private set; }
    public bool IsPrivate { get; private set; }
    public WorkoutStatus Status { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    
    // Routine support
    public bool IsRoutine { get; private set; }
    public string? RoutineName { get; private set; }
    public Guid? CreatedFromRoutineId { get; private set; }

    // Navigation
    public AppUser User { get; private set; } = null!;

    // EF Core constructor
    protected UserWorkout() { }

    protected UserWorkout(Guid id, Guid userId, string workoutType, DateTime date)
    {
        Id = id;
        UserId = userId;
        WorkoutType = workoutType;
        Date = date;
        Status = WorkoutStatus.InProgress;
        CreatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new WorkoutStartedEvent(id, userId, workoutType));
    }

    public void Complete(double? durationMinutes = null)
    {
        if (Status == WorkoutStatus.Completed)
            throw new DomainException("Workout is already completed.");

        Status = WorkoutStatus.Completed;
        CompletedAt = DateTime.UtcNow;
        DurationMinutes = durationMinutes;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new WorkoutCompletedEvent(Id, UserId, WorkoutType, DurationMinutes));
    }

    public void UpdateNotes(string? notes)
    {
        Notes = notes;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetPrivacy(bool isPrivate)
    {
        IsPrivate = isPrivate;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SaveAsRoutine(string routineName)
    {
        if (string.IsNullOrWhiteSpace(routineName))
            throw new DomainException("Routine name cannot be empty.");

        IsRoutine = true;
        RoutineName = routineName;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetCreatedFromRoutine(Guid routineId)
    {
        CreatedFromRoutineId = routineId;
    }
}