using backend.Modules.AiCoaching.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Modules.AiCoaching.Configuration;

public sealed class CoachMessageConfiguration : IEntityTypeConfiguration<CoachMessage>
{
    public void Configure(EntityTypeBuilder<CoachMessage> builder)
    {
        builder.ToTable("CoachMessage");
        builder.HasKey(message => message.Id);

        builder.Property(message => message.Role).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(message => message.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(message => message.Question).HasMaxLength(AiCoachInteractionLimits.MaximumQuestionLength);
        builder.Property(message => message.TimeZoneId).HasMaxLength(100);
        builder.Property(message => message.SourceFingerprint).HasMaxLength(128);
        builder.Property(message => message.SnapshotSchemaVersion).HasMaxLength(60);
        builder.Property(message => message.ContextSnapshotJson).HasColumnType("jsonb");
        builder.Property(message => message.PromptVersion).HasMaxLength(60);
        builder.Property(message => message.ResponseSchemaVersion).HasMaxLength(60);
        builder.Property(message => message.AnswerJson).HasColumnType("jsonb");
        builder.Property(message => message.Provider).HasMaxLength(40);
        builder.Property(message => message.ProviderResponseId).HasMaxLength(200);
        builder.Property(message => message.Model).HasMaxLength(100);
        builder.Property(message => message.LastFailureKind).HasConversion<string>().HasMaxLength(40);
        builder.Property(message => message.LastFailureMessage).HasMaxLength(300);
        builder.Property(message => message.CreatedAt).HasDefaultValueSql("NOW()");
        builder.Property(message => message.ConcurrencyToken).IsConcurrencyToken();

        builder.HasIndex(message => new { message.ThreadId, message.SequenceNumber })
            .IsUnique()
            .HasDatabaseName("UX_CoachMessage_ThreadId_SequenceNumber");
        builder.HasIndex(message => new { message.UserId, message.ClientRequestId })
            .IsUnique()
            .HasFilter("\"ClientRequestId\" IS NOT NULL")
            .HasDatabaseName("UX_CoachMessage_UserId_ClientRequestId");
        builder.HasIndex(message => new { message.Status, message.RequestedAt })
            .HasDatabaseName("IX_CoachMessage_Status_RequestedAt");
        builder.HasIndex(message => new { message.UserId, message.LocalRequestDate })
            .HasDatabaseName("IX_CoachMessage_UserId_LocalRequestDate");
        builder.HasOne(message => message.User)
            .WithMany(user => user.CoachMessages)
            .HasForeignKey(message => message.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
