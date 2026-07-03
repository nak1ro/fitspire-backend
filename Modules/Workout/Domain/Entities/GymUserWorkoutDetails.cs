using backend.Modules.Shared.Domain;
using backend.Modules.Workout.Domain.Enums;

namespace backend.Modules.Workout.Domain.Entities;

public class GymUserWorkoutDetails : UserWorkout
{
    public WorkoutSplit? SplitType { get; private set; }
    public WorkoutIntensity? IntensityLevel { get; private set; }

    private readonly List<GymWorkoutExercise> _exercises = new();
    public IReadOnlyCollection<GymWorkoutExercise> Exercises => _exercises.AsReadOnly();

    // EF Core constructor
    private GymUserWorkoutDetails() { }

    public GymUserWorkoutDetails(Guid id, Guid userId, DateTime date, WorkoutSplit? splitType = null)
        : base(id, userId, "gym", date)
    {
        SplitType = splitType;
    }

    public void SetSplitType(WorkoutSplit? splitType)
    {
        SplitType = splitType;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetIntensity(WorkoutIntensity? level)
    {
        IntensityLevel = level;
        UpdatedAt = DateTime.UtcNow;
    }

    public GymWorkoutExercise AddExercise(Guid exerciseId, int sets, int reps, double weight)
    {
        if (Status == WorkoutStatus.Archived)
            throw new DomainException("Cannot add exercises to an archived workout.");

        var orderIndex = _exercises.Count + 1;
        var exercise = new GymWorkoutExercise(
            Guid.NewGuid(),
            Id,
            exerciseId,
            sets,
            reps,
            weight,
            orderIndex
        );

        _exercises.Add(exercise);
        UpdatedAt = DateTime.UtcNow;

        return exercise;
    }

    public GymWorkoutExercise AddExercise(Guid exerciseId, string? notes = null)
    {
        if (Status == WorkoutStatus.Archived)
            throw new DomainException("Cannot add exercises to an archived workout.");

        var exercise = new GymWorkoutExercise(Guid.NewGuid(), Id, exerciseId, 0, 0, 0, _exercises.Count + 1);
        exercise.UpdateNotes(notes);
        _exercises.Add(exercise);
        UpdatedAt = DateTime.UtcNow;
        return exercise;
    }

    public void UpdateExercise(Guid exerciseEntryId, int? sets = null, int? reps = null, double? weight = null)
    {
        if (Status == WorkoutStatus.Archived)
            throw new DomainException("Cannot update exercises in an archived workout.");

        var exercise = _exercises.FirstOrDefault(e => e.Id == exerciseEntryId)
            ?? throw new DomainException($"Exercise entry {exerciseEntryId} not found.");

        exercise.Update(sets, reps, weight);
        UpdatedAt = DateTime.UtcNow;
    }

    public void RemoveExercise(Guid exerciseEntryId)
    {
        if (Status == WorkoutStatus.Archived)
            throw new DomainException("Cannot remove exercises from an archived workout.");

        var exercise = _exercises.FirstOrDefault(e => e.Id == exerciseEntryId)
            ?? throw new DomainException($"Exercise entry {exerciseEntryId} not found.");

        _exercises.Remove(exercise);
        ReorderExercises();
        UpdatedAt = DateTime.UtcNow;
    }

    public void ReorderExercises(List<Guid> orderedIds)
    {
        if (orderedIds.Count != _exercises.Count || orderedIds.Distinct().Count() != orderedIds.Count)
            throw new DomainException("Must provide all exercise IDs for reordering.");

        for (int i = 0; i < orderedIds.Count; i++)
        {
            var exercise = _exercises.FirstOrDefault(e => e.Id == orderedIds[i])
                ?? throw new DomainException($"Exercise {orderedIds[i]} not found.");
            exercise.SetOrder(i + 1);
        }

        UpdatedAt = DateTime.UtcNow;
    }

    private void ReorderExercises()
    {
        var ordered = _exercises.OrderBy(e => e.OrderIndex).ToList();
        for (int i = 0; i < ordered.Count; i++)
        {
            ordered[i].SetOrder(i + 1);
        }
    }

    public double CalculateTotalVolume() => _exercises.Sum(e => e.CalculateVolume());

    public void ReplaceExercises(IEnumerable<(Guid ExerciseId, int Sets, int Reps, double Weight)> exercises)
    {
        if (Status == WorkoutStatus.Archived)
            throw new DomainException("Cannot edit an archived workout.");
        _exercises.Clear();
        foreach (var (exerciseId, sets, reps, weight) in exercises)
            AddExercise(exerciseId, sets, reps, weight);
    }

    public GymWorkoutExercise FindExercise(Guid exerciseEntryId) => _exercises.FirstOrDefault(exercise => exercise.Id == exerciseEntryId)
        ?? throw new DomainException($"Exercise entry {exerciseEntryId} was not found.");

    public void EnsureLiveSessionMutationAllowed()
    {
        if (!IsActiveSession())
            throw new DomainException("Exercise and set mutations require an active or paused workout session.");
    }

    public bool HasCompletedSets() => _exercises.SelectMany(exercise => exercise.WorkoutSets).Any(set => set.IsCompleted);
    public double? GetMaxWeight() => _exercises.Select(exercise => exercise.GetMaximumCompletedWeight()).Max();
    public override double? GetTotalVolume() => CalculateTotalVolume();
    public override int? GetExerciseCount() => _exercises.Count;

    protected override void EnsureCanComplete()
    {
        if (!HasCompletedSets())
            throw new DomainException("A gym workout requires at least one completed set.");
    }
}
