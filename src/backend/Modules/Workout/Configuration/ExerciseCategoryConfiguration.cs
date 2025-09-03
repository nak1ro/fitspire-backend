using backend.Modules.Workout.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Modules.Workout.Configuration;

public class ExerciseCategoryConfiguration : IEntityTypeConfiguration<ExerciseCategory>
{
    public void Configure(EntityTypeBuilder<ExerciseCategory> builder)
    {
        builder.ToTable("ExerciseCategory");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name).IsRequired();
    }
}