using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenDXP.Domain.Personalization;

namespace OpenDXP.Infrastructure.Persistence.Configurations;

public class PageVariantConfiguration : IEntityTypeConfiguration<PageVariant>
{
    public void Configure(EntityTypeBuilder<PageVariant> builder)
    {
        builder.HasKey(v => v.Id);
        builder.Property(v => v.Id).ValueGeneratedNever();

        builder.Property(v => v.Name).IsRequired().HasMaxLength(200);
        builder.Property(v => v.BlocksJson).IsRequired().HasColumnType("jsonb");
        builder.Property(v => v.TargetSegment).HasMaxLength(100);

        builder.HasIndex(v => v.PageId);
    }
}
