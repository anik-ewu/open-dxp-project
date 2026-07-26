using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OpenDXP.Domain.Auditing;
using OpenDXP.Domain.Content;
using OpenDXP.Infrastructure.Identity;

namespace OpenDXP.Infrastructure.Persistence;

public class OpenDxpDbContext(DbContextOptions<OpenDxpDbContext> options)
    : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>(options)
{
    public DbSet<Page> Pages => Set<Page>();
    public DbSet<AuditLogEntry> AuditLogEntries => Set<AuditLogEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OpenDxpDbContext).Assembly);
        modelBuilder.UseOpenIddict();
    }
}
