using backend.Modules.Group.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Modules.Group.Configuration;

public class GroupMemberConfiguration : IEntityTypeConfiguration<GroupMember>
{
    public void Configure(EntityTypeBuilder<GroupMember> builder)
    {
        builder.ToTable("GroupMember");

        builder.HasKey(gm => gm.Id);

        builder.Property(gm => gm.JoinedAt)
            .HasDefaultValueSql("NOW()");

        builder.HasIndex(gm => new { gm.GroupId, gm.UserId })
            .IsUnique();

        builder.HasOne(gm => gm.UserGroup)
            .WithMany(g => g.Members)
            .HasForeignKey(gm => gm.GroupId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(gm => gm.User)
            .WithMany() // no reverse nav from AppUser for now
            .HasForeignKey(gm => gm.UserId);
    }
}