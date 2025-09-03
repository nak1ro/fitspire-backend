using backend.Modules.Moderation.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Modules.Moderation.Configuration;

public class ReportConfiguration : IEntityTypeConfiguration<Report>
{
    public void Configure(EntityTypeBuilder<Report> builder)
    {
        builder.ToTable("Report");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Reason)
            .IsRequired();

        builder.Property(r => r.CreatedAt)
            .HasDefaultValueSql("NOW()");

        builder.HasOne(r => r.ReportedBy)
            .WithMany(u => u.ReportsMade)
            .HasForeignKey(r => r.ReportedById)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.ReportedUser)
            .WithMany(u => u.ReportsReceived)
            .HasForeignKey(r => r.ReportedUserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}