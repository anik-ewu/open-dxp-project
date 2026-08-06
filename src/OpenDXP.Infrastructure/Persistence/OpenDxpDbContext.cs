using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OpenDXP.Domain.Auditing;
using OpenDXP.Domain.Content;
using OpenDXP.Domain.Outbox;
using OpenDXP.Domain.Personalization;
using OpenDXP.Domain.Search;
using OpenDXP.Infrastructure.Identity;

namespace OpenDXP.Infrastructure.Persistence;

public class OpenDxpDbContext(DbContextOptions<OpenDxpDbContext> options)
    : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>(options)
{
    public DbSet<Page> Pages => Set<Page>();
    public DbSet<AuditLogEntry> AuditLogEntries => Set<AuditLogEntry>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<PageSearchEntry> PageSearchEntries => Set<PageSearchEntry>();
    public DbSet<PageVariant> PageVariants => Set<PageVariant>();
    public DbSet<VariantAnalytics> VariantAnalytics => Set<VariantAnalytics>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OpenDxpDbContext).Assembly);
        modelBuilder.UseOpenIddict();
    }
}
