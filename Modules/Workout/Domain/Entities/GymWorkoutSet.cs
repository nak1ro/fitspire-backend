using backend.Modules.Shared.Domain;

namespace backend.Modules.Workout.Domain.Entities;

public class GymWorkoutSet : Entity<Guid>
{
    public Guid GymWorkoutExerciseId { get; private set; }
    public int OrderIndex { get; private set; }
    public int? Reps { get; private set; }
    public double? WeightKg { get; private set; }
    public int? DurationSeconds { get; private set; }
    public double? DistanceMeters { get; private set; }
    public bool IsWarmup { get; private set; }
    public double? Rpe { get; private set; }
    public string? Notes { get; private set; }
    public bool IsCompleted { get; private set; }
    public DateTime? CompletedAtUtc { get; private set; }

    public GymWorkoutExercise GymWorkoutExercise { get; private set; } = null!;

    private GymWorkoutSet()
    {
    }

    internal static GymWorkoutSet Create(
        Guid gymWorkoutExerciseId,
        int orderIndex,
        int? reps,
        double? weightKg,
        int? durationSeconds,
        double? distanceMeters,
        bool isWarmup,
        double? rpe,
        string? notes,
        bool isCompleted)
    {
        var set = new GymWorkoutSet
        {
            Id = Guid.NewGuid(),
            GymWorkoutExerciseId = gymWorkoutExerciseId,
            OrderIndex = orderIndex,
            CreatedAt = DateTime.UtcNow
        };
        set.Update(reps, weightKg, durationSeconds, distanceMeters, isWarmup, rpe, notes);
        set.SetCompletion(isCompleted, DateTime.UtcNow);
        return set;
    }

    internal void Update(
        int? reps,
        double? weightKg,
        int? durationSeconds,
        double? distanceMeters,
        bool isWarmup,
        double? rpe,
        string? notes)
    {
        ValidateMeasurements(reps, weightKg, durationSeconds, distanceMeters, rpe);
        Reps = reps;
        WeightKg = weightKg;
        DurationSeconds = durationSeconds;
        DistanceMeters = distanceMeters;
        IsWarmup = isWarmup;
        Rpe = rpe;
        Notes = NormalizeNotes(notes);
        if (IsCompleted)
            EnsureCompletedSetHasMeasurement();
        UpdatedAt = DateTime.UtcNow;
    }

    internal void SetCompletion(bool isCompleted, DateTime nowUtc)
    {
        if (isCompleted)
        {
            EnsureCompletedSetHasMeasurement();
            IsCompleted = true;
            CompletedAtUtc ??= nowUtc;
        }
        else
        {
            IsCompleted = false;
            CompletedAtUtc = null;
        }

        UpdatedAt = nowUtc;
    }

    public void SetOrder(int orderIndex)
    {
        if (orderIndex < 0)
            throw new DomainException("Set order cannot be negative.");

        OrderIndex = orderIndex;
        UpdatedAt = DateTime.UtcNow;
    }

    public double CalculateVolume() => IsCompleted && Reps.HasValue && WeightKg.HasValue
        ? Reps.Value * WeightKg.Value
        : 0;

    private static void ValidateMeasurements(int? reps, double? weightKg, int? durationSeconds, double? distanceMeters, double? rpe)
    {
        if (reps is <= 0)
            throw new DomainException("Set reps must be positive when provided.");
        if (weightKg is < 0)
            throw new DomainException("Set weight cannot be negative.");
        if (durationSeconds is <= 0)
            throw new DomainException("Set duration must be positive when provided.");
        if (distanceMeters is <= 0)
            throw new DomainException("Set distance must be positive when provided.");
        if (rpe is < 1 or > 10)
            throw new DomainException("Set RPE must be between 1 and 10.");
    }

    private void EnsureCompletedSetHasMeasurement()
    {
        if (!Reps.HasValue && !DurationSeconds.HasValue && !DistanceMeters.HasValue)
            throw new DomainException("A completed set requires reps, duration, or distance.");
    }

    private static string? NormalizeNotes(string? notes)
    {
        if (notes is null)
            return null;

        var normalized = notes.Trim();
        if (normalized.Length > 500)
            throw new DomainException("Set notes must be at most 500 characters.");

        return string.IsNullOrEmpty(normalized) ? null : normalized;
    }
}
