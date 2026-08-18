using backend.Modules.Workout.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Modules.Workout.Configuration;

public class PersonalRecordConfiguration : IEntityTypeConfiguration<PersonalRecord>
{
    public void Configure(EntityTypeBuilder<PersonalRecord> builder)
    {
        builder.ToTable("PersonalRecord");

        builder.HasKey(pr => pr.Id);

        builder.Property(pr => pr.WorkoutType).IsRequired();
        builder.Property(pr => pr.Metric).IsRequired();
        builder.Property(pr => pr.Value).IsRequired();
        builder.Property(pr => pr.CreatedAt).HasDefaultValueSql("NOW()");
        builder.Property(pr => pr.AchievedAt).IsRequired();
        builder.Property(pr => pr.IsFeatured).HasDefaultValue(false);

        builder.HasOne(pr => pr.User)
            .WithMany(u => u.PersonalRecords)
            .HasForeignKey(pr => pr.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(pr => pr.UserWorkout)
            .WithMany()
            .HasForeignKey(pr => pr.WorkoutId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(pr => pr.Exercise)
            .WithMany()
            .HasForeignKey(pr => pr.ExerciseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(pr => new { pr.UserId, pr.WorkoutType, pr.Metric })
            .IsUnique()
            .HasFilter("\"ExerciseId\" IS NULL");

        builder.HasIndex(pr => new { pr.UserId, pr.WorkoutType, pr.Metric, pr.ExerciseId })
            .IsUnique()
            .HasFilter("\"ExerciseId\" IS NOT NULL");

        builder.HasIndex(pr => pr.UserId)
            .IsUnique()
            .HasFilter("\"IsFeatured\" = true")
            .HasDatabaseName("IX_PersonalRecord_UserId_Featured");
    }
}
