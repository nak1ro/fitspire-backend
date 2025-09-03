using backend.Modules.Goal.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Modules.Goal.Configuration;

public class GoalTypeConfiguration : IEntityTypeConfiguration<GoalType>
{
    public void Configure(EntityTypeBuilder<GoalType> builder)
    {
        builder.ToTable("GoalType");

        builder.HasKey(gt => gt.Id);

        builder.Property(gt => gt.Name)
            .IsRequired();

        builder.Property(gt => gt.Unit);
        builder.Property(gt => gt.Category);
        builder.Property(gt => gt.IconUrl);
        builder.Property(gt => gt.Description);

        builder.HasMany(gt => gt.Goals)
            .WithOne(g => g.GoalType)
            .HasForeignKey(g => g.GoalTypeId);
    }
}