using backend.Modules.Goal.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Modules.Goal.Configuration;

public class GoalConfiguration : IEntityTypeConfiguration<UserGoal>
{
    public void Configure(EntityTypeBuilder<UserGoal> builder)
    {
        builder.ToTable("Goal");

        builder.HasKey(g => g.Id);

        builder.Property(g => g.TargetValue)
            .IsRequired();

        builder.Property(g => g.Unit)
            .IsRequired();

        builder.HasOne(g => g.User)
            .WithMany(u => u.Goals)
            .HasForeignKey(g => g.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(g => g.GoalType)
            .WithMany(gt => gt.Goals)
            .HasForeignKey(g => g.GoalTypeId);
    }
}