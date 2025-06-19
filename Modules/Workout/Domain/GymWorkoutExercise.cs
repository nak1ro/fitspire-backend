namespace backend.Modules.Workout.Domain;

public class GymWorkoutExercise
{
    public Guid Id { get; set; }
    public Guid GymWorkoutId { get; set; }
    public Guid ExerciseId { get; set; }

    public int? Sets { get; set; }
    public int? Reps { get; set; }
    public double? Weight { get; set; }
    public double? DurationMinutes { get; set; }
    public int? OrderIndex { get; set; }

    public GymUserWorkoutDetails GymUserWorkout { get; set; } = null!;
    public Exercise Exercise { get; set; } = null!;
}