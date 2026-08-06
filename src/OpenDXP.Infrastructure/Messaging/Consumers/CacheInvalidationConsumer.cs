using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenDXP.Application.Common.Caching;
using OpenDXP.Application.Common.Messaging;
using OpenDXP.Application.Content.Dtos;
using OpenDXP.Domain.Content;
using StackExchange.Redis;

namespace OpenDXP.Infrastructure.Messaging.Consumers;

/// <summary>Keeps the delivery API's Redis cache fresh: every publish overwrites the cached entry.</summary>
public class CacheInvalidationConsumer(
    IServiceScopeFactory scopeFactory, IConfiguration configuration, ILogger<CacheInvalidationConsumer> logger)
    : KafkaConsumerBackgroundService(scopeFactory, configuration, logger, "opendxp-cache-invalidator", KafkaTopics.ContentEvents)
{
    protected override async Task HandleAsync(
        string eventType, string payloadJson, IServiceProvider scopedProvider, CancellationToken cancellationToken)
    {
        if (eventType != nameof(PagePublishedEvent))
        {
            return;
        }

        var evt = JsonSerializer.Deserialize<PagePublishedEvent>(payloadJson)
                  ?? throw new InvalidOperationException("Could not deserialize PagePublishedEvent.");

        var dto = new PublishedPageDto(evt.Slug, evt.Title, evt.BlocksJson, evt.VersionNumber, evt.OccurredAt);

        var redis = scopedProvider.GetRequiredService<IConnectionMultiplexer>();
        var db = redis.GetDatabase();
        await db.StringSetAsync(CacheKeys.PublishedPage(evt.Slug), JsonSerializer.Serialize(dto));
    }
}
