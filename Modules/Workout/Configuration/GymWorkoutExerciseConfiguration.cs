using backend.Modules.Workout.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Modules.Workout.Configuration;

public class GymWorkoutExerciseConfiguration : IEntityTypeConfiguration<GymWorkoutExercise>
{
    public void Configure(EntityTypeBuilder<GymWorkoutExercise> builder)
    {
        builder.ToTable("GymWorkoutExercise");

        builder.HasKey(e => e.Id);

        builder.HasOne(e => e.Exercise)
            .WithMany(e => e.GymWorkoutExercises)
            .HasForeignKey(e => e.ExerciseId);

        builder.Property(e => e.Sets);
        builder.Property(e => e.Reps);
        builder.Property(e => e.Weight);
        builder.Property(e => e.DurationMinutes);
        builder.Property(e => e.OrderIndex);
    }
}