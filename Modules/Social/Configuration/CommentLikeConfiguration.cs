using backend.Modules.Social.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Modules.Social.Configuration;

public class CommentLikeConfiguration : IEntityTypeConfiguration<CommentLike>
{
    public void Configure(EntityTypeBuilder<CommentLike> builder)
    {
        builder.ToTable("CommentLike");
        builder.HasKey(like => like.Id);

        builder.Property(like => like.CreatedAt)
            .HasDefaultValueSql("NOW()");

        builder.HasOne(like => like.User)
            .WithMany(user => user.CommentLikes)
            .HasForeignKey(like => like.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(like => like.Comment)
            .WithMany(comment => comment.Likes)
            .HasForeignKey(like => like.CommentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(like => new { like.UserId, like.CommentId }).IsUnique();
    }
}
