using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenDXP.Application.Common.Auditing;
using OpenDXP.Application.Content;
using OpenDXP.Infrastructure.Auditing;
using OpenDXP.Infrastructure.Messaging;
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

        var bootstrapServers = configuration["Kafka:BootstrapServers"] ?? "localhost:19092";
        services.AddSingleton<IProducer<string, string>>(_ =>
            new ProducerBuilder<string, string>(new ProducerConfig { BootstrapServers = bootstrapServers }).Build());
        services.AddHostedService<OutboxPublisherService>();

        return services;
    }
}
