using backend.Modules.Friendship.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Modules.Friendship.Configuration;

public class FriendshipRequestConfiguration : IEntityTypeConfiguration<FriendshipRequest>
{
    public void Configure(EntityTypeBuilder<FriendshipRequest> builder)
    {
        builder.ToTable("FriendshipRequest");

        builder.HasKey(r => r.Id);

        builder.HasOne(r => r.Requester)
            .WithMany(u => u.FriendshipSentRequests)
            .HasForeignKey(r => r.RequesterId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.Addressee)
            .WithMany(u => u.FriendshipReceivedRequests)
            .HasForeignKey(r => r.AddresseeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(r => r.Status)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(r => r.RequestedAt)
            .HasDefaultValueSql("NOW()");

        builder.HasCheckConstraint("CK_FriendshipRequest_Status", "\"Status\" IN ('pending', 'accepted', 'rejected')");

        builder.HasIndex(r => new { r.RequesterId, r.AddresseeId })
            .IsUnique();
    }
}