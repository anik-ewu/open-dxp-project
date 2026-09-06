using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using OpenDXP.Application.Common.Auditing;
using OpenDXP.Application.Content;
using OpenDXP.Infrastructure.Auditing;
using OpenDXP.Infrastructure.Messaging;
using OpenDXP.Infrastructure.Messaging.Consumers;
using OpenDXP.Infrastructure.Outbox;
using OpenDXP.Infrastructure.Persistence;
using OpenDXP.Infrastructure.Personalization;
using OpenDXP.Infrastructure.Search;
using OpenDXP.Application.Personalization;
using OpenDXP.Application.Search;
using Pgvector.Npgsql;
using StackExchange.Redis;

namespace OpenDXP.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<DomainEventsToOutboxInterceptor>();

        var dataSourceBuilder = new NpgsqlDataSourceBuilder(configuration.GetConnectionString("Postgres"));
        dataSourceBuilder.UseVector();
        services.AddSingleton(dataSourceBuilder.Build());

        services.AddDbContext<OpenDxpDbContext>((sp, options) =>
        {
            options.UseNpgsql(sp.GetRequiredService<NpgsqlDataSource>());
            options.UseOpenIddict();
            options.AddInterceptors(sp.GetRequiredService<DomainEventsToOutboxInterceptor>());
        });

        services.AddScoped<IPageRepository, PageRepository>();
        services.AddScoped<IAuditLogService, AuditLogService>();
        services.AddScoped<ISearchRepository, SearchRepository>();
        services.AddScoped<IPageVariantRepository, PageVariantRepository>();
        services.AddScoped<IVariantAnalyticsRepository, VariantAnalyticsRepository>();
        services.AddSingleton<IEmbeddingService, HashingEmbeddingService>();

        services.AddOpenIddict()
            .AddCore(options => options.UseEntityFrameworkCore().UseDbContext<OpenDxpDbContext>());

        services.AddSingleton<IConnectionMultiplexer>(_ =>
            ConnectionMultiplexer.Connect(configuration.GetConnectionString("Redis") ?? "localhost:6379"));

        var bootstrapServers = configuration["Kafka:BootstrapServers"] ?? "localhost:19092";
        services.AddSingleton<IProducer<string, string>>(_ =>
            new ProducerBuilder<string, string>(new ProducerConfig { BootstrapServers = bootstrapServers }).Build());
        services.AddHostedService<OutboxPublisherService>();
        services.AddHostedService<CacheInvalidationConsumer>();
        services.AddHostedService<SearchReindexConsumer>();
        services.AddHostedService<AuditTrailConsumer>();
        services.AddHostedService<AnalyticsAggregatorConsumer>();
        services.AddHostedService<AutoTaggingConsumer>();

        return services;
    }
}
