using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using backend.Modules.Workout.Domain;

namespace backend.Modules.Workout.Configuration;

public class UserWorkoutConfiguration : IEntityTypeConfiguration<UserWorkout>
{
    public void Configure(EntityTypeBuilder<UserWorkout> builder)
    {
        builder.ToTable("UserWorkout");

        builder.HasKey(w => w.Id);

        builder.Property(w => w.WorkoutType).IsRequired();
        builder.Property(w => w.Date).IsRequired();
        builder.Property(w => w.IsPrivate).HasDefaultValue(false);
        builder.Property(w => w.IsRoutine).HasDefaultValue(false);
        builder.Property(w => w.CreatedAt).HasDefaultValueSql("NOW()");

        builder.HasOne(w => w.User)
            .WithMany(u => u.Workouts)
            .HasForeignKey(w => w.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}