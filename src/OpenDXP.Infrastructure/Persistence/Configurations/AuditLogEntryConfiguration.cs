using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenDXP.Domain.Auditing;

namespace OpenDXP.Infrastructure.Persistence.Configurations;

public class AuditLogEntryConfiguration : IEntityTypeConfiguration<AuditLogEntry>
{
    public void Configure(EntityTypeBuilder<AuditLogEntry> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();

        builder.Property(e => e.EventType).IsRequired().HasMaxLength(100);
        builder.Property(e => e.Subject).HasMaxLength(256);
        builder.Property(e => e.Detail).IsRequired().HasMaxLength(1000);
        builder.Property(e => e.IpAddress).HasMaxLength(64);

        builder.HasIndex(e => e.CreatedAt);
    }
}
