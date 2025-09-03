namespace backend.Modules.Workout.Domain;

public class SwimmingUserWorkoutDetails : UserWorkout
{
    public int? Laps { get; set; }
    public double? DistanceMeters { get; set; }
    public string? StrokeType { get; set; }
}