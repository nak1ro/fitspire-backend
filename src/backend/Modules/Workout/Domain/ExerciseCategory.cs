namespace backend.Modules.Workout.Domain;

public class ExerciseCategory
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }

    public ICollection<Exercise> Exercises { get; set; } = new List<Exercise>();
}