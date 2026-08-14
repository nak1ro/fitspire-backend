using backend.Modules.Moderation.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Modules.Moderation.Configuration;

public sealed class ModerationReportConfiguration : IEntityTypeConfiguration<ModerationReport>
{
    public void Configure(EntityTypeBuilder<ModerationReport> builder)
    {
        builder.ToTable("ModerationReport");
        builder.HasKey(report => report.Id);

        builder.Property(report => report.TargetType).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(report => report.MediaContext).HasConversion<string>().HasMaxLength(20);
        builder.Property(report => report.Reason).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(report => report.Details).HasMaxLength(ModerationLimits.MaximumReportDetailsLength);
        builder.Property(report => report.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(report => report.ResolutionOutcome).HasConversion<string>().HasMaxLength(20);
        builder.Property(report => report.ResolutionNote).HasMaxLength(ModerationLimits.MaximumResolutionNoteLength);
        builder.Property(report => report.TargetSnapshotJson).HasColumnType("jsonb").IsRequired();
        builder.Property(report => report.SnapshotVersion).HasMaxLength(60).IsRequired();
        builder.Property(report => report.CreatedAt).HasDefaultValueSql("NOW()");
        builder.Property(report => report.ConcurrencyToken).IsConcurrencyToken();

        builder.HasIndex(report => new { report.ReporterUserId, report.TargetType, report.TargetId })
            .IsUnique()
            .HasFilter("\"Status\" = 'Open'")
            .HasDatabaseName("UX_ModerationReport_OpenReporterTarget");
        builder.HasIndex(report => new { report.Status, report.CreatedAt })
            .HasDatabaseName("IX_ModerationReport_Status_CreatedAt");
        builder.HasIndex(report => new { report.Status, report.TargetType, report.CreatedAt })
            .HasDatabaseName("IX_ModerationReport_Status_TargetType_CreatedAt");
        builder.HasIndex(report => new { report.SubjectUserId, report.CreatedAt })
            .HasDatabaseName("IX_ModerationReport_SubjectUserId_CreatedAt");

        builder.HasOne(report => report.ReporterUser)
            .WithMany(user => user.ReportsSubmitted)
            .HasForeignKey(report => report.ReporterUserId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(report => report.SubjectUser)
            .WithMany(user => user.ReportsReceived)
            .HasForeignKey(report => report.SubjectUserId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(report => report.ResolvedByUser)
            .WithMany(user => user.ReportsResolved)
            .HasForeignKey(report => report.ResolvedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(report => report.Actions)
            .WithOne(action => action.Report)
            .HasForeignKey(action => action.ReportId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
