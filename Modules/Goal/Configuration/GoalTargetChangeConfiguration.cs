using backend.Modules.Goal.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Modules.Goal.Configuration;

public class GoalTargetChangeConfiguration : IEntityTypeConfiguration<GoalTargetChange>
{
    public void Configure(EntityTypeBuilder<GoalTargetChange> builder)
    {
        builder.ToTable("GoalTargetChange");
        builder.HasKey(change => change.Id);
        builder.Property(change => change.ChangedAt).IsRequired();
        builder.HasIndex(change => new { change.GoalId, change.ChangedAt });
        builder.HasOne(change => change.Goal).WithMany(goal => goal.TargetChanges)
            .HasForeignKey(change => change.GoalId).OnDelete(DeleteBehavior.Cascade);
    }
}
