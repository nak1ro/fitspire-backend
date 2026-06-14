using backend.Modules.Social.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Modules.Social.Configuration;

public class PostLikeConfiguration : IEntityTypeConfiguration<PostLike>
{
    public void Configure(EntityTypeBuilder<PostLike> builder)
    {
        builder.ToTable("PostLike");
        builder.HasKey(like => like.Id);

        builder.Property(like => like.CreatedAt)
            .HasDefaultValueSql("NOW()");

        builder.HasOne(like => like.User)
            .WithMany(user => user.PostLikes)
            .HasForeignKey(like => like.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(like => like.Post)
            .WithMany(post => post.Likes)
            .HasForeignKey(like => like.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(like => new { like.UserId, like.PostId }).IsUnique();
    }
}
