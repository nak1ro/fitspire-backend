using backend.Modules.Shared.Domain;

namespace backend.Modules.Workout.Domain.Entities;

public class GymWorkoutExercise : Entity<Guid>
{
    public Guid GymWorkoutId { get; private set; }
    public Guid ExerciseId { get; private set; }
    public int OrderIndex { get; private set; }
    public string? Notes { get; private set; }
    public ICollection<GymWorkoutSet> WorkoutSets { get; private set; } = new List<GymWorkoutSet>();

    public GymUserWorkoutDetails GymUserWorkout { get; private set; } = null!;
    public Exercise Exercise { get; private set; } = null!;

    private GymWorkoutExercise() { }

    public GymWorkoutExercise(Guid id, Guid gymWorkoutId, Guid exerciseId, int orderIndex)
    {
        if (orderIndex < 1)
            throw new DomainException("Exercise order must be positive.");

        Id = id;
        GymWorkoutId = gymWorkoutId;
        ExerciseId = exerciseId;
        OrderIndex = orderIndex;
        CreatedAt = DateTime.UtcNow;
    }

    public void SetOrder(int newIndex)
    {
        if (newIndex < 1)
            throw new DomainException("Exercise order must be positive.");

        OrderIndex = newIndex;
        UpdatedAt = DateTime.UtcNow;
    }

    public GymWorkoutSet AddSet(int? reps, double? weightKg, int? durationSeconds, double? distanceMeters,
        bool isWarmup, double? rpe, string? notes, bool isCompleted)
    {
        var set = GymWorkoutSet.Create(Id, WorkoutSets.Count, reps, weightKg, durationSeconds, distanceMeters,
            isWarmup, rpe, notes, isCompleted);
        WorkoutSets.Add(set);
        UpdatedAt = DateTime.UtcNow;
        return set;
    }

    public void UpdateSet(Guid setId, int? reps, double? weightKg, int? durationSeconds, double? distanceMeters,
        bool isWarmup, double? rpe, string? notes)
    {
        FindSet(setId).Update(reps, weightKg, durationSeconds, distanceMeters, isWarmup, rpe, notes);
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetSetCompletion(Guid setId, bool isCompleted, DateTime nowUtc)
    {
        FindSet(setId).SetCompletion(isCompleted, nowUtc);
        UpdatedAt = nowUtc;
    }

    public void RemoveSet(Guid setId)
    {
        WorkoutSets.Remove(FindSet(setId));
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

    public double? GetMaximumCompletedWeight() => WorkoutSets.Where(set => set.IsCompleted && set.WeightKg.HasValue)
        .Select(set => set.WeightKg).Max();

    private GymWorkoutSet FindSet(Guid setId) => WorkoutSets.FirstOrDefault(set => set.Id == setId)
        ?? throw new DomainException($"Set {setId} was not found.");

    private void NormalizeSetOrder()
    {
        foreach (var (set, index) in WorkoutSets.OrderBy(set => set.OrderIndex).Select((set, index) => (set, index)))
            set.SetOrder(index);
    }
}
