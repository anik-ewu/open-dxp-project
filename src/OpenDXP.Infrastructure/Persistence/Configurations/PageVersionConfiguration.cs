using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenDXP.Domain.Content;

namespace OpenDXP.Infrastructure.Persistence.Configurations;

public class PageVersionConfiguration : IEntityTypeConfiguration<PageVersion>
{
    public void Configure(EntityTypeBuilder<PageVersion> builder)
    {
        builder.HasKey(v => v.Id);
        builder.Property(v => v.Id).ValueGeneratedNever();

        builder.Property(v => v.Title)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(v => v.BlocksJson)
            .HasColumnType("jsonb")
            .IsRequired();

        builder.HasIndex(v => new { v.PageId, v.VersionNumber }).IsUnique();
    }
}
