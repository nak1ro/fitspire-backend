using backend.Modules.User.Domain;

namespace backend.Modules.Workout.Domain;

public class PersonalRecord
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string WorkoutType { get; set; } = null!;  // e.g. "gym", "running"
    public string Metric { get; set; } = null!;       // e.g. "1RM", "MaxDistanceKm"
    public double Value { get; set; }
    public Guid WorkoutId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public AppUser User { get; set; } = null!;
    public UserWorkout UserWorkout { get; set; } = null!;
}