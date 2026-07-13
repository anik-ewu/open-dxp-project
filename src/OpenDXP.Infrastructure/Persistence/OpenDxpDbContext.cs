using Microsoft.EntityFrameworkCore;
using OpenDXP.Domain.Content;

namespace OpenDXP.Infrastructure.Persistence;

public class OpenDxpDbContext(DbContextOptions<OpenDxpDbContext> options) : DbContext(options)
{
    public DbSet<Page> Pages => Set<Page>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OpenDxpDbContext).Assembly);
    }
}
