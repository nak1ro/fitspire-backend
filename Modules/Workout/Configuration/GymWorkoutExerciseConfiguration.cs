using backend.Modules.Workout.Domain.Entities;
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

        builder.Property(e => e.OrderIndex);
        builder.Property(e => e.Notes).HasMaxLength(500);

    }
}
