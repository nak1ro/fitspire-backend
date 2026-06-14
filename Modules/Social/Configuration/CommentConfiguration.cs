using backend.Modules.Social.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Modules.Social.Configuration;

public class CommentConfiguration : IEntityTypeConfiguration<Comment>
{
    public void Configure(EntityTypeBuilder<Comment> builder)
    {
        builder.ToTable("Comment");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.CreatedAt)
            .HasDefaultValueSql("NOW()");

        builder.HasOne(c => c.Post)
            .WithMany(p => p.Comments)
            .HasForeignKey(c => c.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.User)
            .WithMany(u => u.Comments)
            .HasForeignKey(c => c.UserId);

        builder.HasOne(c => c.RootComment)
            .WithMany()
            .HasForeignKey(c => c.RootCommentId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(c => c.ReplyToComment)
            .WithMany(c => c.Replies)
            .HasForeignKey(c => c.ReplyToCommentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(c => new { c.PostId, c.RootCommentId, c.CreatedAt });
    }
}
