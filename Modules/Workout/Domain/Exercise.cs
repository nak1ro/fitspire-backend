namespace backend.Modules.Workout.Domain;

public class Exercise
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public Guid? CategoryId { get; set; }

    public ExerciseCategory? Category { get; set; }

    public ICollection<GymWorkoutExercise> GymWorkoutExercises { get; set; } = new List<GymWorkoutExercise>();
}