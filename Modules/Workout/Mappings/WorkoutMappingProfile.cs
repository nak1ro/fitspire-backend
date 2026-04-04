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
            .ForMember(d => d.Status, opt => opt.MapFrom(s => s.Status.ToString()))
            .ForMember(d => d.SplitType, opt => opt.MapFrom(s => s.SplitType.HasValue ? s.SplitType.ToString() : null))
            .ForMember(d => d.IntensityLevel, opt => opt.MapFrom(s => s.IntensityLevel.HasValue ? s.IntensityLevel.ToString() : null));

        CreateMap<GymWorkoutExercise, GymExerciseResponse>()
            .ForMember(d => d.ExerciseName, opt => opt.MapFrom(s => s.Exercise.Name));

        CreateMap<ExerciseCategory, ExerciseCategoryResponse>()
            .ForCtorParam(nameof(ExerciseCategoryResponse.ExercisesCount), opt => opt.MapFrom(s => s.Exercises.Count));

        CreateMap<Exercise, ExerciseResponse>()
            .ForCtorParam(nameof(ExerciseResponse.CategoryName), opt => opt.MapFrom(s => s.Category != null ? s.Category.Name : null));

        CreateMap<WorkoutRoutine, WorkoutRoutineResponse>();

        CreateMap<PersonalRecord, PersonalRecordResponse>()
            .ForCtorParam(nameof(PersonalRecordResponse.AchievedAt), opt => opt.MapFrom(s => s.UpdatedAt ?? s.CreatedAt));
            
        // Running
        CreateMap<RunningUserWorkoutDetails, RunningWorkoutResponse>()
            .ForMember(d => d.Status, opt => opt.MapFrom(s => s.Status.ToString()));

        // Cycling
        CreateMap<CyclingUserWorkoutDetails, CyclingWorkoutResponse>()
            .ForMember(d => d.Status, opt => opt.MapFrom(s => s.Status.ToString()));

        // Swimming
        CreateMap<SwimmingUserWorkoutDetails, SwimmingWorkoutResponse>()
            .ForMember(d => d.Status, opt => opt.MapFrom(s => s.Status.ToString()))
            .ForMember(d => d.StrokeType, opt => opt.MapFrom(s => s.StrokeType.HasValue ? s.StrokeType.ToString() : null));

         // Yoga
        CreateMap<YogaUserWorkoutDetails, YogaWorkoutResponse>()
            .ForMember(d => d.Status, opt => opt.MapFrom(s => s.Status.ToString()))
            .ForMember(d => d.Style, opt => opt.MapFrom(s => s.Style.HasValue ? s.Style.ToString() : null))
            .ForMember(d => d.Intensity, opt => opt.MapFrom(s => s.Intensity.HasValue ? s.Intensity.ToString() : null))
            .ForMember(d => d.FocusArea, opt => opt.MapFrom(s => s.FocusArea.HasValue ? s.FocusArea.ToString() : null));

        // Base
        CreateMap<UserWorkout, WorkoutResponse>()
            .ForCtorParam(nameof(WorkoutResponse.Status), opt => opt.MapFrom(s => s.Status.ToString()))
            .ForCtorParam(nameof(WorkoutResponse.IsRoutine), opt => opt.MapFrom(_ => false))
            .ForCtorParam(nameof(WorkoutResponse.RoutineName), opt => opt.MapFrom(_ => (string?)null));
    }
}
