using backend.Modules.AiCoaching.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Modules.AiCoaching.Configuration;

public sealed class CoachThreadConfiguration : IEntityTypeConfiguration<CoachThread>
{
    public void Configure(EntityTypeBuilder<CoachThread> builder)
    {
        builder.ToTable("CoachThread");
        builder.HasKey(thread => thread.Id);

        builder.Property(thread => thread.Title).HasMaxLength(AiCoachInteractionLimits.MaximumThreadTitleLength).IsRequired();
        builder.Property(thread => thread.ContextSummary).HasMaxLength(AiCoachInteractionLimits.MaximumThreadSummaryLength);
        builder.Property(thread => thread.LastActivityAt).IsRequired();
        builder.Property(thread => thread.CreatedAt).HasDefaultValueSql("NOW()");
        builder.Property(thread => thread.ConcurrencyToken).IsConcurrencyToken();

        builder.HasIndex(thread => new { thread.UserId, thread.LastActivityAt })
            .HasDatabaseName("IX_CoachThread_UserId_LastActivityAt");
        builder.HasOne(thread => thread.User)
            .WithMany(user => user.CoachThreads)
            .HasForeignKey(thread => thread.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(thread => thread.Messages)
            .WithOne(message => message.Thread)
            .HasForeignKey(message => message.ThreadId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
