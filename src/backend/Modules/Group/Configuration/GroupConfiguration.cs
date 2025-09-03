using backend.Modules.Group.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Modules.Group.Configuration;

public class GroupConfiguration : IEntityTypeConfiguration<UserGroup>
{
    public void Configure(EntityTypeBuilder<UserGroup> builder)
    {
        builder.ToTable("Group");

        builder.HasKey(g => g.Id);

        builder.Property(g => g.Name)
            .IsRequired();

        builder.Property(g => g.Description);

        builder.Property(g => g.IsPrivate)
            .HasDefaultValue(false);

        builder.Property(g => g.CreatedAt)
            .HasDefaultValueSql("NOW()");

        builder.HasOne(g => g.CreatedBy)
            .WithMany() // no reverse nav from AppUser
            .HasForeignKey(g => g.CreatedById)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(g => g.Members)
            .WithOne(m => m.UserGroup)
            .HasForeignKey(m => m.GroupId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}