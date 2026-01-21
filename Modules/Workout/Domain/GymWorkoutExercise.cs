using backend.Modules.Shared.Domain;

namespace backend.Modules.Workout.Domain;

public class GymWorkoutExercise : Entity<Guid>
{
    public Guid GymWorkoutId { get; private set; }
    public Guid ExerciseId { get; private set; }
    public int Sets { get; private set; }
    public int Reps { get; private set; }
    public double Weight { get; private set; }
    public double? DurationMinutes { get; private set; }
    public int OrderIndex { get; private set; }

    // Navigation
    public GymUserWorkoutDetails GymUserWorkout { get; private set; } = null!;
    public Exercise Exercise { get; private set; } = null!;

    // EF Core constructor
    private GymWorkoutExercise() { }

    public GymWorkoutExercise(
        Guid id,
        Guid gymWorkoutId,
        Guid exerciseId,
        int sets,
        int reps,
        double weight,
        int orderIndex)
    {
        if (sets <= 0) throw new DomainException("Sets must be greater than 0.");
        if (reps <= 0) throw new DomainException("Reps must be greater than 0.");
        if (weight < 0) throw new DomainException("Weight cannot be negative.");

        Id = id;
        GymWorkoutId = gymWorkoutId;
        ExerciseId = exerciseId;
        Sets = sets;
        Reps = reps;
        Weight = weight;
        OrderIndex = orderIndex;
        CreatedAt = DateTime.UtcNow;
    }

    public void Update(int? sets = null, int? reps = null, double? weight = null, double? durationMinutes = null)
    {
        if (sets.HasValue)
        {
            if (sets.Value <= 0) throw new DomainException("Sets must be greater than 0.");
            Sets = sets.Value;
        }
        
        if (reps.HasValue)
        {
            if (reps.Value <= 0) throw new DomainException("Reps must be greater than 0.");
            Reps = reps.Value;
        }
        
        if (weight.HasValue)
        {
            if (weight.Value < 0) throw new DomainException("Weight cannot be negative.");
            Weight = weight.Value;
        }

        if (durationMinutes.HasValue)
            DurationMinutes = durationMinutes.Value;

        UpdatedAt = DateTime.UtcNow;
    }

    public void SetOrder(int newIndex)
    {
        OrderIndex = newIndex;
        UpdatedAt = DateTime.UtcNow;
    }

    public double CalculateVolume() => Sets * Reps * Weight;
}