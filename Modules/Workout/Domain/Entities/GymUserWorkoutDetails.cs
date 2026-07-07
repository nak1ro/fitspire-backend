using backend.Modules.Shared.Domain;
using backend.Modules.Workout.Domain.Enums;

namespace backend.Modules.Workout.Domain.Entities;

public class GymUserWorkoutDetails : UserWorkout
{
    public WorkoutSplit? SplitType { get; private set; }
    public WorkoutIntensity? IntensityLevel { get; private set; }

    private readonly List<GymWorkoutExercise> _exercises = new();
    public IReadOnlyCollection<GymWorkoutExercise> Exercises => _exercises.AsReadOnly();

    private GymUserWorkoutDetails() { }

    public GymUserWorkoutDetails(Guid id, Guid userId, DateTime date, WorkoutSplit? splitType = null)
        : base(id, userId, "gym", date) => SplitType = splitType;

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

    public GymWorkoutExercise AddExercise(Guid exerciseId, string? notes = null)
    {
        EnsureNotArchived("add exercises to");
        var exercise = new GymWorkoutExercise(Guid.NewGuid(), Id, exerciseId, _exercises.Count + 1);
        exercise.UpdateNotes(notes);
        _exercises.Add(exercise);
        UpdatedAt = DateTime.UtcNow;
        return exercise;
    }

    public void RemoveExercise(Guid exerciseEntryId)
    {
        EnsureNotArchived("remove exercises from");
        _exercises.Remove(FindExercise(exerciseEntryId));
        NormalizeExerciseOrder();
        UpdatedAt = DateTime.UtcNow;
    }

    public void ReorderExercises(IReadOnlyList<Guid> orderedIds)
    {
        if (orderedIds.Count != _exercises.Count || orderedIds.Distinct().Count() != orderedIds.Count)
            throw new DomainException("Exercise reorder must contain every exercise exactly once.");

        for (var index = 0; index < orderedIds.Count; index++)
            FindExercise(orderedIds[index]).SetOrder(index + 1);

        UpdatedAt = DateTime.UtcNow;
    }

    public void ClearExercises()
    {
        EnsureNotArchived("edit");
        _exercises.Clear();
        UpdatedAt = DateTime.UtcNow;
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
    public override double? GetTotalVolume() => _exercises.Sum(exercise => exercise.CalculateCompletedSetVolume());
    public override int? GetExerciseCount() => _exercises.Count;

    protected override void EnsureCanComplete()
    {
        if (!HasCompletedSets())
            throw new DomainException("A gym workout requires at least one completed set.");
    }

    private void EnsureNotArchived(string action)
    {
        if (Status == WorkoutStatus.Archived)
            throw new DomainException($"Cannot {action} an archived workout.");
    }

    private void NormalizeExerciseOrder()
    {
        foreach (var (exercise, index) in _exercises.OrderBy(exercise => exercise.OrderIndex).Select((exercise, index) => (exercise, index)))
            exercise.SetOrder(index + 1);
    }
}
