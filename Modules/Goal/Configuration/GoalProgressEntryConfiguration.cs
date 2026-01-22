using backend.Modules.Goal.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Modules.Goal.Configuration;

public class GoalProgressEntryConfiguration : IEntityTypeConfiguration<GoalProgressEntry>
{
    public void Configure(EntityTypeBuilder<GoalProgressEntry> builder)
    {
        builder.ToTable("GoalProgressEntry");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.RecordedAt).IsRequired();
        builder.Property(p => p.Source).HasMaxLength(50);

        builder.HasOne(p => p.Goal)
            .WithMany(g => g.ProgressEntries)
            .HasForeignKey(p => p.GoalId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
