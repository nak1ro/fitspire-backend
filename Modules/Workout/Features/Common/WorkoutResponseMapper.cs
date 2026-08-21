using AutoMapper;
using backend.Modules.Workout.Domain.Entities;
using backend.Modules.Workout.DTOs;

namespace backend.Modules.Workout.Features.Common;

// Shared by WorkoutController (owner viewing their own workout) and the Social
// module's public workout detail query (another user viewing a shared workout).
public static class WorkoutResponseMapper
{
    public static object Map(UserWorkout workout, IMapper mapper) => workout switch
    {
        GymUserWorkoutDetails gymWorkout => mapper.Map<GymWorkoutResponse>(gymWorkout),
        RunningUserWorkoutDetails runningWorkout => mapper.Map<RunningWorkoutResponse>(runningWorkout),
        CyclingUserWorkoutDetails cyclingWorkout => mapper.Map<CyclingWorkoutResponse>(cyclingWorkout),
        SwimmingUserWorkoutDetails swimmingWorkout => mapper.Map<SwimmingWorkoutResponse>(swimmingWorkout),
        YogaUserWorkoutDetails yogaWorkout => mapper.Map<YogaWorkoutResponse>(yogaWorkout),
        _ => mapper.Map<WorkoutResponse>(workout)
    };
}
