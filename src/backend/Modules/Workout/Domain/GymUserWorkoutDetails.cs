namespace backend.Modules.Workout.Domain;

public class GymUserWorkoutDetails : UserWorkout
{
    public string? SplitType { get; set; }
    public string? IntensityLevel { get; set; }

    public ICollection<GymWorkoutExercise> Exercises { get; set; } = new List<GymWorkoutExercise>();
}