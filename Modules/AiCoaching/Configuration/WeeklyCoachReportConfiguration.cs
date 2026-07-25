using backend.Modules.AiCoaching.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Modules.AiCoaching.Configuration;

public sealed class WeeklyCoachReportConfiguration : IEntityTypeConfiguration<WeeklyCoachReport>
{
    public void Configure(EntityTypeBuilder<WeeklyCoachReport> builder)
    {
        builder.ToTable("WeeklyCoachReport");
        builder.HasKey(report => report.Id);

        builder.Property(report => report.TimeZoneId).HasMaxLength(100).IsRequired();
        builder.Property(report => report.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(report => report.GenerationCount).IsRequired();
        builder.Property(report => report.SourceFingerprint).HasMaxLength(128).IsRequired();
        builder.Property(report => report.SnapshotSchemaVersion).HasMaxLength(60).IsRequired();
        builder.Property(report => report.SnapshotJson).HasColumnType("jsonb").IsRequired();
        builder.Property(report => report.PromptVersion).HasMaxLength(60).IsRequired();
        builder.Property(report => report.ResponseSchemaVersion).HasMaxLength(60).IsRequired();
        builder.Property(report => report.ReportJson).HasColumnType("jsonb");
        builder.Property(report => report.Provider).HasMaxLength(40);
        builder.Property(report => report.ProviderResponseId).HasMaxLength(200);
        builder.Property(report => report.Model).HasMaxLength(100);
        builder.Property(report => report.LastFailureKind).HasConversion<string>().HasMaxLength(40);
        builder.Property(report => report.LastFailureMessage).HasMaxLength(300);
        builder.Property(report => report.CreatedAt).HasDefaultValueSql("NOW()");
        builder.Property(report => report.ConcurrencyToken).IsConcurrencyToken();

        builder.HasIndex(report => new { report.UserId, report.PeriodStart })
            .IsUnique()
            .HasDatabaseName("UX_WeeklyCoachReport_UserId_PeriodStart");
        builder.HasIndex(report => new { report.Status, report.RequestedAt })
            .HasDatabaseName("IX_WeeklyCoachReport_Status_RequestedAt");
        builder.HasOne(report => report.User)
            .WithMany(user => user.WeeklyCoachReports)
            .HasForeignKey(report => report.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
