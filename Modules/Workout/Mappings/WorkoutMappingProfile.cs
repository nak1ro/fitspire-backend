using AutoMapper;
using backend.Modules.Workout.Domain.Entities;
using backend.Modules.Workout.DTOs;

namespace backend.Modules.Workout.Mappings;

public class WorkoutMappingProfile : Profile
{
    public WorkoutMappingProfile()
    {
        // Gym
        CreateMap<GymUserWorkoutDetails, GymWorkoutResponse>()
            .IncludeBase<UserWorkout, WorkoutResponse>()
            .ForMember(d => d.SplitType, opt => opt.MapFrom(s => s.SplitType.HasValue ? s.SplitType.ToString() : null))
            .ForMember(d => d.IntensityLevel, opt => opt.MapFrom(s => s.IntensityLevel.HasValue ? s.IntensityLevel.ToString() : null));

        CreateMap<GymWorkoutExercise, GymExerciseResponse>()
            .ForMember(d => d.ExerciseName, opt => opt.MapFrom(s => s.Exercise.Name));
            
        // Running
        CreateMap<RunningUserWorkoutDetails, RunningWorkoutResponse>()
            .IncludeBase<UserWorkout, WorkoutResponse>();

        // Cycling
        CreateMap<CyclingUserWorkoutDetails, CyclingWorkoutResponse>()
            .IncludeBase<UserWorkout, WorkoutResponse>();

        // Swimming
        CreateMap<SwimmingUserWorkoutDetails, SwimmingWorkoutResponse>()
            .IncludeBase<UserWorkout, WorkoutResponse>()
            .ForMember(d => d.StrokeType, opt => opt.MapFrom(s => s.StrokeType.HasValue ? s.StrokeType.ToString() : null));

         // Yoga
        CreateMap<YogaUserWorkoutDetails, YogaWorkoutResponse>()
            .IncludeBase<UserWorkout, WorkoutResponse>()
            .ForMember(d => d.Style, opt => opt.MapFrom(s => s.Style.HasValue ? s.Style.ToString() : null))
            .ForMember(d => d.Intensity, opt => opt.MapFrom(s => s.Intensity.HasValue ? s.Intensity.ToString() : null))
            .ForMember(d => d.FocusArea, opt => opt.MapFrom(s => s.FocusArea.HasValue ? s.FocusArea.ToString() : null));

        // Base
        CreateMap<UserWorkout, WorkoutResponse>()
            .ForMember(d => d.Status, opt => opt.MapFrom(s => s.Status.ToString()));
    }
}
