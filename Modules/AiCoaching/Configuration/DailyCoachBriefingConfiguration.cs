using backend.Modules.AiCoaching.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Modules.AiCoaching.Configuration;

public sealed class DailyCoachBriefingConfiguration : IEntityTypeConfiguration<DailyCoachBriefing>
{
    public void Configure(EntityTypeBuilder<DailyCoachBriefing> builder)
    {
        builder.ToTable("DailyCoachBriefing");
        builder.HasKey(briefing => briefing.Id);

        builder.Property(briefing => briefing.TimeZoneId).HasMaxLength(100).IsRequired();
        builder.Property(briefing => briefing.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(briefing => briefing.SourceFingerprint).HasMaxLength(128);
        builder.Property(briefing => briefing.SnapshotSchemaVersion).HasMaxLength(60);
        builder.Property(briefing => briefing.SnapshotJson).HasColumnType("jsonb");
        builder.Property(briefing => briefing.PromptVersion).HasMaxLength(60);
        builder.Property(briefing => briefing.ResponseSchemaVersion).HasMaxLength(60);
        builder.Property(briefing => briefing.ContentJson).HasColumnType("jsonb");
        builder.Property(briefing => briefing.Provider).HasMaxLength(40);
        builder.Property(briefing => briefing.ProviderResponseId).HasMaxLength(200);
        builder.Property(briefing => briefing.Model).HasMaxLength(100);
        builder.Property(briefing => briefing.LastFailureKind).HasConversion<string>().HasMaxLength(40);
        builder.Property(briefing => briefing.LastFailureMessage).HasMaxLength(300);
        builder.Property(briefing => briefing.RefreshCount).HasDefaultValue(0);
        builder.Property(briefing => briefing.CreatedAt).HasDefaultValueSql("NOW()");
        builder.Property(briefing => briefing.ConcurrencyToken).IsConcurrencyToken();

        builder.HasIndex(briefing => new { briefing.UserId, briefing.LocalDate })
            .IsUnique()
            .HasDatabaseName("UX_DailyCoachBriefing_UserId_LocalDate");
        builder.HasIndex(briefing => new { briefing.Status, briefing.RequestedAt })
            .HasDatabaseName("IX_DailyCoachBriefing_Status_RequestedAt");
        builder.HasOne(briefing => briefing.User)
            .WithMany(user => user.DailyCoachBriefings)
            .HasForeignKey(briefing => briefing.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
