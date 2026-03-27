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
    public DateTime? CompletedAt { get; private set; }
    
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

        AddDomainEvent(new WorkoutCompletedEvent(
            Id, 
            UserId, 
            WorkoutType, 
            IsPrivate,
            DurationMinutes,
            GetTotalDistance(),
            CaloriesBurned,
            GetTotalVolume()
        ));
    }

    // Virtual metric getters for subclasses to override
    public virtual double? GetTotalDistance() => null;
    public virtual double? GetTotalVolume() => null;

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
        AddDomainEvent(new WorkoutDeletedEvent(Id, UserId, WorkoutType));
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
