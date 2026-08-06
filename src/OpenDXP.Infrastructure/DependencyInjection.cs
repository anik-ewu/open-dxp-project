using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenDXP.Application.Common.Auditing;
using OpenDXP.Application.Content;
using OpenDXP.Infrastructure.Auditing;
using OpenDXP.Infrastructure.Outbox;
using OpenDXP.Infrastructure.Persistence;

namespace OpenDXP.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<DomainEventsToOutboxInterceptor>();

        services.AddDbContext<OpenDxpDbContext>((sp, options) =>
        {
            options.UseNpgsql(configuration.GetConnectionString("Postgres"));
            options.UseOpenIddict();
            options.AddInterceptors(sp.GetRequiredService<DomainEventsToOutboxInterceptor>());
        });

        services.AddScoped<IPageRepository, PageRepository>();
        services.AddScoped<IAuditLogService, AuditLogService>();

        services.AddOpenIddict()
            .AddCore(options => options.UseEntityFrameworkCore().UseDbContext<OpenDxpDbContext>());

        return services;
    }
}
