using backend.Modules.Progress.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Modules.Progress.Configuration;

public class MetricDefinitionConfiguration : IEntityTypeConfiguration<MetricDefinition>
{
    public void Configure(EntityTypeBuilder<MetricDefinition> builder)
    {
        builder.ToTable("MetricDefinition");
        builder.HasKey(metric => metric.Id);
        builder.Property(metric => metric.Id).HasMaxLength(80);
        builder.Property(metric => metric.DisplayName).IsRequired().HasMaxLength(120);
        builder.Property(metric => metric.CanonicalUnit).IsRequired().HasMaxLength(40);
        builder.Property(metric => metric.Aggregation).IsRequired().HasMaxLength(24);
    }
}
