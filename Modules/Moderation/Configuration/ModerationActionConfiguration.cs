using backend.Modules.Moderation.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Modules.Moderation.Configuration;

public sealed class ModerationActionConfiguration : IEntityTypeConfiguration<ModerationAction>
{
    public void Configure(EntityTypeBuilder<ModerationAction> builder)
    {
        builder.ToTable("ModerationAction");
        builder.HasKey(action => action.Id);

        builder.Property(action => action.TargetType).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(action => action.ActionType).HasConversion<string>().HasMaxLength(24).IsRequired();
        builder.Property(action => action.Note).HasMaxLength(ModerationLimits.MaximumActionNoteLength);
        builder.Property(action => action.OccurredAtUtc).IsRequired();
        builder.Property(action => action.CreatedAt).HasDefaultValueSql("NOW()");

        builder.HasIndex(action => new { action.ReportId, action.OccurredAtUtc })
            .HasDatabaseName("IX_ModerationAction_ReportId_OccurredAtUtc");

        builder.HasOne(action => action.ModeratorUser)
            .WithMany(user => user.ModerationActions)
            .HasForeignKey(action => action.ModeratorUserId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(action => action.SubjectUser)
            .WithMany()
            .HasForeignKey(action => action.SubjectUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
