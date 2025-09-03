namespace backend.Modules.Workout.Domain;

public class YogaUserWorkoutDetails : UserWorkout
{
    public string? Style { get; set; }
    public string? Intensity { get; set; }
    public string? FocusArea { get; set; }
}