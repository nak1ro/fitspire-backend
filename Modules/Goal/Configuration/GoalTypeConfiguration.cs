using backend.Modules.Goal.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Modules.Goal.Configuration;

public class GoalTypeConfiguration : IEntityTypeConfiguration<GoalType>
{
    public void Configure(EntityTypeBuilder<GoalType> builder)
    {
        builder.ToTable("GoalType");

        builder.HasKey(gt => gt.Id);

        builder.Property(gt => gt.Name).HasMaxLength(100).IsRequired();
        builder.Property(gt => gt.DefaultUnit).HasMaxLength(50).IsRequired();
        builder.Property(gt => gt.Description).HasMaxLength(500);
        builder.Property(gt => gt.IconUrl).HasMaxLength(255);
        builder.Property(gt => gt.Category).HasConversion<string>();
        builder.Property(gt => gt.MeasurementType).HasConversion<string>();
        builder.Property(gt => gt.RelatedWorkoutType).HasMaxLength(50);
        builder.Property(gt => gt.RelatedMetric).HasMaxLength(50);

        builder.HasMany(gt => gt.Goals)
            .WithOne(g => g.GoalType)
            .HasForeignKey(g => g.GoalTypeId);
    }
}