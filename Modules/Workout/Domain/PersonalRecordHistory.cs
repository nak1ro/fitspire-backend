using backend.Modules.User.Domain;

namespace backend.Modules.Workout.Domain;

public class PersonalRecordHistory
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string WorkoutType { get; set; } = null!;
    public string Metric { get; set; } = null!;
    public double Value { get; set; }
    public Guid? WorkoutId { get; set; }
    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public AppUser User { get; set; } = null!;
    public UserWorkout? Workout { get; set; }
}