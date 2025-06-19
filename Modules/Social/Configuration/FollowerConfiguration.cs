using backend.Modules.Social.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Modules.Social.Configuration;

public class FollowerConfiguration : IEntityTypeConfiguration<Follower>
{
    public void Configure(EntityTypeBuilder<Follower> builder)
    {
        builder.ToTable("Follower");

        builder.HasKey(f => f.Id);

        builder.Property(f => f.CreatedAt)
            .HasDefaultValueSql("NOW()");

        builder.HasOne(f => f.FollowerUser)
            .WithMany(u => u.Following)
            .HasForeignKey(f => f.FollowerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(f => f.FollowedUser)
            .WithMany(u => u.Followers)
            .HasForeignKey(f => f.FollowedId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(f => new { f.FollowerId, f.FollowedId }).IsUnique();

        builder.HasCheckConstraint("CK_Follower_NotSelfFollow", "\"FollowerId\" <> \"FollowedId\"");
    }
}