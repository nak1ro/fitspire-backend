using backend.Modules.Workout.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Modules.Workout.Configuration;

public class PersonalRecordHistoryConfiguration : IEntityTypeConfiguration<PersonalRecordHistory>
{
    public void Configure(EntityTypeBuilder<PersonalRecordHistory> builder)
    {
        builder.ToTable("PersonalRecordHistory");

        builder.HasKey(prh => prh.Id);

        builder.Property(prh => prh.WorkoutType).IsRequired();
        builder.Property(prh => prh.Metric).IsRequired();
        builder.Property(prh => prh.Value).IsRequired();
        builder.Property(prh => prh.RecordedAt).HasDefaultValueSql("NOW()");

        builder.HasOne(prh => prh.User)
            .WithMany(u => u.RecordHistory)
            .HasForeignKey(prh => prh.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(prh => prh.Workout)
            .WithMany()
            .HasForeignKey(prh => prh.WorkoutId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}