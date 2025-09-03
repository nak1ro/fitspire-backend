using backend.Modules.User.Domain;

namespace backend.Modules.Workout.Domain;

public abstract class UserWorkout
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string WorkoutType { get; set; } = null!;
    public DateTime Date { get; set; }
    public double? DurationMinutes { get; set; }
    public string? Notes { get; set; }
    public bool IsPrivate { get; set; } = false;
    public bool IsRoutine { get; set; } = false;
    public string? RoutineName { get; set; }
    public Guid? CreatedFromRoutineId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Guid? CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }

    public AppUser User { get; set; } = null!;
}