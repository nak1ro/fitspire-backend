using backend.Modules.Goal.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Modules.Goal.Configuration;

public class UserGoalConfiguration : IEntityTypeConfiguration<UserGoal>
{
    public void Configure(EntityTypeBuilder<UserGoal> builder)
    {
        builder.ToTable("UserGoal");

        builder.HasKey(g => g.Id);

        builder.Property(g => g.TargetValue).IsRequired();
        builder.Property(g => g.CurrentValue).IsRequired();
        builder.Property(g => g.Unit).HasMaxLength(50).IsRequired();
        builder.Property(g => g.Status).HasConversion<string>();
        builder.Property(g => g.RecurrencePattern).HasMaxLength(50);
        builder.Property(g => g.TimeZoneId).HasMaxLength(128).IsRequired();
        builder.Property(g => g.SelectedWorkoutType).HasMaxLength(32);

        builder.HasOne(g => g.User)
            .WithMany(u => u.Goals)
            .HasForeignKey(g => g.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(g => g.GoalType)
            .WithMany(gt => gt.Goals)
            .HasForeignKey(g => g.GoalTypeId);

        builder.HasMany(g => g.ProgressEntries)
            .WithOne(pe => pe.Goal)
            .HasForeignKey(pe => pe.GoalId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(g => g.Periods)
            .WithOne(period => period.Goal)
            .HasForeignKey(period => period.GoalId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
