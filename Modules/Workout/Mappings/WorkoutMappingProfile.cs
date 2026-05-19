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
            .ForCtorParam(nameof(GymWorkoutResponse.Status), opt => opt.MapFrom(s => s.Status.ToString()))
            .ForCtorParam(nameof(GymWorkoutResponse.IsRoutine), opt => opt.MapFrom(s => s.CreatedFromRoutineId.HasValue))
            .ForCtorParam(nameof(GymWorkoutResponse.RoutineName), opt => opt.MapFrom(_ => (string?)null))
            .ForCtorParam(nameof(GymWorkoutResponse.SplitType), opt => opt.MapFrom(s => s.SplitType.HasValue ? s.SplitType.ToString() : null))
            .ForCtorParam(nameof(GymWorkoutResponse.IntensityLevel), opt => opt.MapFrom(s => s.IntensityLevel.HasValue ? s.IntensityLevel.ToString() : null))
            .ForCtorParam(nameof(GymWorkoutResponse.Exercises), opt => opt.MapFrom(s => s.Exercises.OrderBy(e => e.OrderIndex)));

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
            .ForCtorParam(nameof(RunningWorkoutResponse.Status), opt => opt.MapFrom(s => s.Status.ToString()));

        // Cycling
        CreateMap<CyclingUserWorkoutDetails, CyclingWorkoutResponse>()
            .ForCtorParam(nameof(CyclingWorkoutResponse.Status), opt => opt.MapFrom(s => s.Status.ToString()));

        // Swimming
        CreateMap<SwimmingUserWorkoutDetails, SwimmingWorkoutResponse>()
            .ForCtorParam(nameof(SwimmingWorkoutResponse.Status), opt => opt.MapFrom(s => s.Status.ToString()))
            .ForCtorParam(nameof(SwimmingWorkoutResponse.StrokeType), opt => opt.MapFrom(s => s.StrokeType.HasValue ? s.StrokeType.ToString() : null));

         // Yoga
        CreateMap<YogaUserWorkoutDetails, YogaWorkoutResponse>()
            .ForCtorParam(nameof(YogaWorkoutResponse.Status), opt => opt.MapFrom(s => s.Status.ToString()))
            .ForCtorParam(nameof(YogaWorkoutResponse.Style), opt => opt.MapFrom(s => s.Style.HasValue ? s.Style.ToString() : null))
            .ForCtorParam(nameof(YogaWorkoutResponse.Intensity), opt => opt.MapFrom(s => s.Intensity.HasValue ? s.Intensity.ToString() : null))
            .ForCtorParam(nameof(YogaWorkoutResponse.FocusArea), opt => opt.MapFrom(s => s.FocusArea.HasValue ? s.FocusArea.ToString() : null));

        // Base
        CreateMap<UserWorkout, WorkoutResponse>()
            .ForCtorParam(nameof(WorkoutResponse.Status), opt => opt.MapFrom(s => s.Status.ToString()))
            .ForCtorParam(nameof(WorkoutResponse.IsRoutine), opt => opt.MapFrom(_ => false))
            .ForCtorParam(nameof(WorkoutResponse.RoutineName), opt => opt.MapFrom(_ => (string?)null));
    }
}
