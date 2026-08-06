using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenDXP.Domain.Personalization;

namespace OpenDXP.Infrastructure.Persistence.Configurations;

public class VariantAnalyticsConfiguration : IEntityTypeConfiguration<VariantAnalytics>
{
    public void Configure(EntityTypeBuilder<VariantAnalytics> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).ValueGeneratedNever();

        builder.Property(a => a.VariantLabel).IsRequired().HasMaxLength(200);

        builder.HasIndex(a => new { a.PageId, a.VariantId }).IsUnique();
    }
}
