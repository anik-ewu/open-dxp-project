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

        // The pgvector EF Core plugin requires EF Core 9, which conflicts with the rest of this
        // solution's EF 8 stack (Identity, OpenIddict). Excluded from the model here and managed
        // entirely via raw SQL (SearchRepository) instead - Npgsql's own vector type handling
        // (UseVector() on the data source) still works fine for parameterized raw SQL.
        builder.Ignore(e => e.Embedding);
    }
}
