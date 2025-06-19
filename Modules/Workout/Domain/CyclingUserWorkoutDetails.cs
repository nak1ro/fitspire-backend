namespace backend.Modules.Workout.Domain;

public class CyclingUserWorkoutDetails : UserWorkout
{
    public double? DistanceKm { get; set; }
    public double? ElevationGain { get; set; }
    public double? AvgSpeedKmPerHour { get; set; }
}
