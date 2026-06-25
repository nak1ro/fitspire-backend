using backend.Modules.Progress.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Modules.Progress.Configuration;

public class ActivityContributionConfiguration : IEntityTypeConfiguration<ActivityContribution>
{
    public void Configure(EntityTypeBuilder<ActivityContribution> builder)
    {
        builder.ToTable("ActivityContribution");
        builder.HasKey(contribution => contribution.Id);
        builder.Property(contribution => contribution.MetricCode).IsRequired().HasMaxLength(80);
        builder.Property(contribution => contribution.WorkoutType).IsRequired().HasMaxLength(32);
        builder.HasIndex(contribution => new { contribution.SourceWorkoutId, contribution.MetricCode, contribution.ExerciseId })
            .IsUnique();
        builder.HasIndex(contribution => new { contribution.UserId, contribution.MetricCode, contribution.OccurredAt });
        builder.HasIndex(contribution => new { contribution.UserId, contribution.WorkoutType, contribution.OccurredAt });
    }
}
