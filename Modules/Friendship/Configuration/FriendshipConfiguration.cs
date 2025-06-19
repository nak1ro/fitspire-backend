using backend.Modules.Friendship.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Modules.Friendship.Configuration;

public class FriendshipConfiguration : IEntityTypeConfiguration<FriendshipConnection>
{
    public void Configure(EntityTypeBuilder<FriendshipConnection> builder)
    {
        builder.ToTable("Friendship");

        builder.HasKey(f => f.Id);

        builder.HasOne(f => f.User1)
            .WithMany(u => u.Friends)
            .HasForeignKey(f => f.User1Id)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(f => f.User2)
            .WithMany()
            .HasForeignKey(f => f.User2Id)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(f => f.BecameFriendsAt)
            .HasDefaultValueSql("NOW()");

        builder.HasCheckConstraint("CK_Friendship_NoSelf", "\"User1Id\" <> \"User2Id\"");

        builder.HasIndex(f => new { f.User1Id, f.User2Id })
            .IsUnique();
    }
}