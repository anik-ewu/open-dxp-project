using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Npgsql;
using Pgvector.Npgsql;

namespace OpenDXP.Infrastructure.Persistence;

/// <summary>
/// Lets `dotnet ef migrations` resolve a DbContext at design time without spinning up the Api host.
/// Not used at runtime — the Api registers OpenDxpDbContext via DependencyInjection.AddInfrastructure.
/// </summary>
public class OpenDxpDbContextFactory : IDesignTimeDbContextFactory<OpenDxpDbContext>
{
    public OpenDxpDbContext CreateDbContext(string[] args)
    {
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(
            "Host=localhost;Database=opendxp;Username=opendxp;Password=opendxp");
        dataSourceBuilder.UseVector();

        var optionsBuilder = new DbContextOptionsBuilder<OpenDxpDbContext>();
        optionsBuilder.UseNpgsql(dataSourceBuilder.Build());
        return new OpenDxpDbContext(optionsBuilder.Options);
    }
}
