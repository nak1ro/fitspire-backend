using backend.Modules.Goal.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Modules.Goal.Configuration;

public class GoalPeriodConfiguration : IEntityTypeConfiguration<GoalPeriod>
{
    public void Configure(EntityTypeBuilder<GoalPeriod> builder)
    {
        builder.ToTable("GoalPeriod");
        builder.HasKey(period => period.Id);
        builder.Property(period => period.Status).HasMaxLength(16).IsRequired();
        builder.HasIndex(period => new { period.GoalId, period.StartAt }).IsUnique();
        builder.HasIndex(period => new { period.Status, period.EndAt });
    }
}
