using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenDXP.Domain.Search;

namespace OpenDXP.Infrastructure.Persistence.Configurations;

public class PageSearchEntryConfiguration : IEntityTypeConfiguration<PageSearchEntry>
{
    public void Configure(EntityTypeBuilder<PageSearchEntry> builder)
    {
        builder.HasKey(e => e.PageId);
        builder.Property(e => e.PageId).ValueGeneratedNever();

        builder.Property(e => e.Slug).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Title).IsRequired().HasMaxLength(500);
        builder.Property(e => e.PlainText).IsRequired();
    }
}
