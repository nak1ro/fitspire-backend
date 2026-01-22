using AutoMapper;
using backend.Modules.Workout.Domain.Entities;
using backend.Modules.Workout.DTOs;

namespace backend.Modules.Workout.Mappings;

public class WorkoutMappingProfile : Profile
{
    public WorkoutMappingProfile()
    {
        // UserWorkout -> WorkoutResponse
        CreateMap<UserWorkout, WorkoutResponse>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

        // GymUserWorkoutDetails -> GymWorkoutResponse
        CreateMap<GymUserWorkoutDetails, GymWorkoutResponse>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.Exercises, opt => opt.MapFrom(src => src.Exercises));

        // GymWorkoutExercise -> GymExerciseResponse
        CreateMap<GymWorkoutExercise, GymExerciseResponse>()
            .ForMember(dest => dest.ExerciseName, opt => opt.MapFrom(src => 
                src.Exercise != null ? src.Exercise.Name : "Unknown"));

        // RunningUserWorkoutDetails -> RunningWorkoutResponse
        CreateMap<RunningUserWorkoutDetails, RunningWorkoutResponse>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

        // CyclingUserWorkoutDetails -> CyclingWorkoutResponse
        CreateMap<CyclingUserWorkoutDetails, CyclingWorkoutResponse>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

        // SwimmingUserWorkoutDetails -> SwimmingWorkoutResponse
        CreateMap<SwimmingUserWorkoutDetails, SwimmingWorkoutResponse>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

        // YogaUserWorkoutDetails -> YogaWorkoutResponse
        CreateMap<YogaUserWorkoutDetails, YogaWorkoutResponse>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.Style, opt => opt.MapFrom(src => src.Style.HasValue ? src.Style.Value.ToString() : null))
            .ForMember(dest => dest.Intensity, opt => opt.MapFrom(src => src.Intensity.HasValue ? src.Intensity.Value.ToString() : null))
            .ForMember(dest => dest.FocusArea, opt => opt.MapFrom(src => src.FocusArea.HasValue ? src.FocusArea.Value.ToString() : null));
    }
}
