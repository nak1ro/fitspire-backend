using backend.Modules.Shared.Domain;
using backend.Modules.User.Domain;
using backend.Modules.Workout.Domain.Events;
using backend.Modules.Workout.Domain.Enums;

namespace backend.Modules.Workout.Domain.Entities;

public abstract class UserWorkout : AggregateRoot<Guid>
{
    public Guid UserId { get; private set; }
    public string WorkoutType { get; private set; } = null!;
    public DateTime Date { get; private set; }
    public double? DurationMinutes { get; private set; }
    public string? Notes { get; private set; }
    public bool IsPrivate { get; private set; }
    public WorkoutStatus Status { get; private set; }
    public DateTime? StartedAt { get; private set; }
    public DateTime? PausedAt { get; private set; }
    public int AccumulatedPausedSeconds { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public DateTime? DeletedAt { get; private set; }
    
    // Common stats
    public int? CaloriesBurned { get; private set; }
    
    // Routine support
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
        Date = NormalizeDate(date);
        Status = WorkoutStatus.InProgress;
        StartedAt = DateTime.UtcNow;
        CreatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new WorkoutStartedEvent(id, userId, workoutType));
    }

    public void Complete(double? durationMinutes = null, string? notes = null, bool? isPrivate = null)
    {
        if (Status == WorkoutStatus.Completed)
            throw new DomainException("Workout is already completed.");

        if (Status == WorkoutStatus.Archived)
            throw new DomainException("An archived workout cannot be completed.");

        if (Status == WorkoutStatus.Paused)
            Resume(DateTime.UtcNow);

        EnsureCanComplete();

        Status = WorkoutStatus.Completed;
        CompletedAt = DateTime.UtcNow;
        DurationMinutes = durationMinutes ?? CalculateElapsedMinutes(CompletedAt.Value);
        if (notes != null)
            Notes = notes;
        if (isPrivate.HasValue)
            IsPrivate = isPrivate.Value;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new WorkoutCompletedEvent(
            Id,
            UserId,
            WorkoutType,
            IsPrivate,
            DurationMinutes,
            GetTotalDistance(),
            CaloriesBurned,
            GetTotalVolume(),
            Notes
        ));
    }

    // Virtual metric getters for subclasses to override
    public virtual double? GetTotalDistance() => null;
    public virtual double? GetTotalVolume() => null;
    public virtual int? GetExerciseCount() => null;
    protected virtual void EnsureCanComplete()
    {
    }

    public void UpdateNotes(string? notes)
    {
        Notes = notes;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetCalories(int? calories)
    {
        if (calories.HasValue && calories.Value < 0)
            throw new DomainException("Calories cannot be negative.");
            
        CaloriesBurned = calories;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetPrivacy(bool isPrivate)
    {
        IsPrivate = isPrivate;
        UpdatedAt = DateTime.UtcNow;
    }



    public void SetCreatedFromRoutine(Guid routineId)
    {
        CreatedFromRoutineId = routineId;
    }

    public void UpdateDetails(DateTime? date, double? duration, string? notes, bool? isPrivate)
    {
        if (Status == WorkoutStatus.Archived)
            throw new DomainException("An archived workout cannot be edited.");

        if (date.HasValue)
            Date = NormalizeDate(date.Value);
        
        if (duration.HasValue)
        {
            if (duration.Value < 0) throw new DomainException("Duration cannot be negative.");
            DurationMinutes = duration.Value;
        }

        if (notes != null)
            Notes = notes;

        if (isPrivate.HasValue)
            IsPrivate = isPrivate.Value;

        UpdatedAt = DateTime.UtcNow;
    }

    public void Delete()
    {
        if (Status == WorkoutStatus.Archived)
            throw new DomainException("Workout is already archived.");

        DeletedAt = DateTime.UtcNow;
        Status = WorkoutStatus.Archived;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new WorkoutDeletedEvent(Id, UserId, WorkoutType));
    }

    public void Pause(DateTime nowUtc)
    {
        if (Status != WorkoutStatus.InProgress)
            throw new DomainException("Only an active workout can be paused.");

        PausedAt = nowUtc;
        Status = WorkoutStatus.Paused;
        UpdatedAt = nowUtc;
    }

    public void Resume(DateTime nowUtc)
    {
        if (Status != WorkoutStatus.Paused || PausedAt is null)
            throw new DomainException("Only a paused workout can be resumed.");

        AccumulatedPausedSeconds += Math.Max(0, (int)(nowUtc - PausedAt.Value).TotalSeconds);
        PausedAt = null;
        Status = WorkoutStatus.InProgress;
        UpdatedAt = nowUtc;
    }

    public void Restore(DateTime nowUtc)
    {
        if (Status != WorkoutStatus.Archived || DeletedAt is null)
            throw new DomainException("Only an archived workout can be restored.");

        DeletedAt = null;
        Status = CompletedAt.HasValue ? WorkoutStatus.Completed : WorkoutStatus.InProgress;
        if (!CompletedAt.HasValue)
        {
            StartedAt = nowUtc;
            PausedAt = null;
            AccumulatedPausedSeconds = 0;
        }

        UpdatedAt = nowUtc;
    }

    public void Abandon()
    {
        if (!IsActiveSession())
            throw new DomainException("Only an active workout session can be abandoned.");

        DeletedAt = DateTime.UtcNow;
        Status = WorkoutStatus.Archived;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool IsActiveSession() => Status is WorkoutStatus.InProgress or WorkoutStatus.Paused;

    private double CalculateElapsedMinutes(DateTime completedAtUtc)
    {
        if (StartedAt is null)
            return DurationMinutes ?? 0;

        var pausedSeconds = AccumulatedPausedSeconds;
        if (PausedAt.HasValue)
            pausedSeconds += Math.Max(0, (int)(completedAtUtc - PausedAt.Value).TotalSeconds);

        return Math.Max(0, (completedAtUtc - StartedAt.Value).TotalMinutes - pausedSeconds / 60d);
    }

    private static DateTime NormalizeDate(DateTime date)
    {
        return date.Kind switch
        {
            DateTimeKind.Utc => date,
            DateTimeKind.Local => date.ToUniversalTime(),
            _ => DateTime.SpecifyKind(date, DateTimeKind.Utc)
        };
    }
}
