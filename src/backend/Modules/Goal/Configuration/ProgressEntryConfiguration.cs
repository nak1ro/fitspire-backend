using backend.Modules.Goal.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Modules.Goal.Configuration;

public class ProgressEntryConfiguration : IEntityTypeConfiguration<ProgressEntry>
{
    public void Configure(EntityTypeBuilder<ProgressEntry> builder)
    {
        builder.ToTable("ProgressEntry");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Date).IsRequired();

        builder.HasOne(p => p.User)
            .WithMany(u => u.ProgressEntries)
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.Goal)
            .WithMany(g => g.ProgressEntries)
            .HasForeignKey(p => p.GoalId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}