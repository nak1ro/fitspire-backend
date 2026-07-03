using backend.Modules.Shared.Domain;

namespace backend.Modules.Workout.Domain.Entities;

public class GymWorkoutExercise : Entity<Guid>
{
    public Guid GymWorkoutId { get; private set; }
    public Guid ExerciseId { get; private set; }
    public int Sets { get; private set; }
    public int Reps { get; private set; }
    public double Weight { get; private set; }
    public double? DurationMinutes { get; private set; }
    public int OrderIndex { get; private set; }
    public string? Notes { get; private set; }
    public ICollection<GymWorkoutSet> WorkoutSets { get; private set; } = new List<GymWorkoutSet>();

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
        if (sets < 0) throw new DomainException("Sets cannot be negative.");
        if (sets > 0 && reps <= 0) throw new DomainException("Reps must be greater than 0 when sets are provided.");
        if (weight < 0) throw new DomainException("Weight cannot be negative.");

        Id = id;
        GymWorkoutId = gymWorkoutId;
        ExerciseId = exerciseId;
        Sets = sets;
        Reps = reps;
        Weight = weight;
        OrderIndex = orderIndex;
        CreatedAt = DateTime.UtcNow;
        if (sets > 0)
            WorkoutSets.Add(GymWorkoutSet.Create(
                Id, 0, reps, weight, null, null, false, null, null, true));
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

        if (WorkoutSets.Count == 1)
        {
            var set = WorkoutSets.Single();
            set.Update(
                reps ?? set.Reps,
                weight ?? set.WeightKg,
                durationMinutes.HasValue ? (int?)Math.Round(durationMinutes.Value * 60) : set.DurationSeconds,
                set.DistanceMeters,
                set.IsWarmup,
                set.Rpe,
                set.Notes);
        }

        UpdatedAt = DateTime.UtcNow;
    }

    public void SetOrder(int newIndex)
    {
        OrderIndex = newIndex;
        UpdatedAt = DateTime.UtcNow;
    }

    public GymWorkoutSet AddSet(
        int? reps,
        double? weightKg,
        int? durationSeconds,
        double? distanceMeters,
        bool isWarmup,
        double? rpe,
        string? notes,
        bool isCompleted)
    {
        var set = GymWorkoutSet.Create(
            Id,
            WorkoutSets.Count,
            reps,
            weightKg,
            durationSeconds,
            distanceMeters,
            isWarmup,
            rpe,
            notes,
            isCompleted);
        WorkoutSets.Add(set);
        UpdatedAt = DateTime.UtcNow;
        return set;
    }

    public void UpdateSet(
        Guid setId,
        int? reps,
        double? weightKg,
        int? durationSeconds,
        double? distanceMeters,
        bool isWarmup,
        double? rpe,
        string? notes)
    {
        var set = FindSet(setId);
        set.Update(reps, weightKg, durationSeconds, distanceMeters, isWarmup, rpe, notes);
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetSetCompletion(Guid setId, bool isCompleted, DateTime nowUtc)
    {
        FindSet(setId).SetCompletion(isCompleted, nowUtc);
        UpdatedAt = nowUtc;
    }

    public void RemoveSet(Guid setId)
    {
        var set = FindSet(setId);
        WorkoutSets.Remove(set);
        NormalizeSetOrder();
        UpdatedAt = DateTime.UtcNow;
    }

    public void ReorderSets(IReadOnlyList<Guid> orderedSetIds)
    {
        if (orderedSetIds.Count != WorkoutSets.Count || orderedSetIds.Distinct().Count() != orderedSetIds.Count)
            throw new DomainException("Set reorder must contain every set exactly once.");

        for (var index = 0; index < orderedSetIds.Count; index++)
            FindSet(orderedSetIds[index]).SetOrder(index);

        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateNotes(string? notes)
    {
        if (notes is not null && notes.Trim().Length > 500)
            throw new DomainException("Exercise notes must be at most 500 characters.");

        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    public double CalculateCompletedSetVolume() => WorkoutSets.Sum(set => set.CalculateVolume());

    public double? GetMaximumCompletedWeight() => WorkoutSets
        .Where(set => set.IsCompleted && set.WeightKg.HasValue)
        .Select(set => set.WeightKg)
        .Max();

    public double CalculateVolume() => WorkoutSets.Count == 0 ? Sets * Reps * Weight : CalculateCompletedSetVolume();

    private GymWorkoutSet FindSet(Guid setId) => WorkoutSets.FirstOrDefault(set => set.Id == setId)
        ?? throw new DomainException($"Set {setId} was not found.");

    private void NormalizeSetOrder()
    {
        foreach (var (set, index) in WorkoutSets.OrderBy(set => set.OrderIndex).Select((set, index) => (set, index)))
            set.SetOrder(index);
    }
}
