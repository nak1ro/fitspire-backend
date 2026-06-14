using backend.Modules.Social.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Modules.Social.Configuration;

public class FollowRequestConfiguration : IEntityTypeConfiguration<FollowRequest>
{
    public void Configure(EntityTypeBuilder<FollowRequest> builder)
    {
        builder.ToTable("FollowRequest", table =>
        {
            table.HasCheckConstraint(
                "CK_FollowRequest_Status",
                "\"Status\" IN ('Pending', 'Accepted', 'Rejected', 'Cancelled')");
            table.HasCheckConstraint("CK_FollowRequest_NoSelf", "\"RequesterId\" <> \"AddresseeId\"");
        });

        builder.HasKey(fr => fr.Id);

        builder.HasOne(fr => fr.Requester)
            .WithMany()
            .HasForeignKey(fr => fr.RequesterId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(fr => fr.Addressee)
            .WithMany()
            .HasForeignKey(fr => fr.AddresseeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(fr => fr.Status)
            .HasConversion<string>()
            .HasMaxLength(16)
            .IsRequired();

        builder.Property(fr => fr.RequestedAt)
            .HasDefaultValueSql("NOW()");

        builder.HasIndex(fr => new { fr.RequesterId, fr.AddresseeId })
            .IsUnique()
            .HasFilter("\"Status\" = 'Pending'");
    }
}
