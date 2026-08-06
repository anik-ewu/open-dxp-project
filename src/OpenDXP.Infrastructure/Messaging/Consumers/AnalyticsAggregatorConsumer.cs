using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenDXP.Application.Common.Messaging;
using OpenDXP.Domain.Personalization;
using OpenDXP.Infrastructure.Persistence;

namespace OpenDXP.Infrastructure.Messaging.Consumers;

/// <summary>Rolls VariantServed/ConversionRecorded events up into VariantAnalytics counters.</summary>
public class AnalyticsAggregatorConsumer(
    IServiceScopeFactory scopeFactory, IConfiguration configuration, ILogger<AnalyticsAggregatorConsumer> logger)
    : KafkaConsumerBackgroundService(scopeFactory, configuration, logger, "opendxp-analytics-aggregator", KafkaTopics.AnalyticsEvents)
{
    protected override async Task HandleAsync(
        string eventType, string payloadJson, IServiceProvider scopedProvider, CancellationToken cancellationToken)
    {
        var dbContext = scopedProvider.GetRequiredService<OpenDxpDbContext>();

        switch (eventType)
        {
            case nameof(VariantServedEvent):
            {
                var evt = JsonSerializer.Deserialize<VariantServedEvent>(payloadJson)
                          ?? throw new InvalidOperationException("Could not deserialize VariantServedEvent.");
                var row = await GetOrCreateAsync(dbContext, evt.PageId, evt.VariantId, evt.VariantLabel, cancellationToken);
                row.RecordImpression();
                break;
            }

            case nameof(ConversionRecordedEvent):
            {
                var evt = JsonSerializer.Deserialize<ConversionRecordedEvent>(payloadJson)
                          ?? throw new InvalidOperationException("Could not deserialize ConversionRecordedEvent.");
                var label = evt.VariantId is null ? "default" : "variant";
                var row = await GetOrCreateAsync(dbContext, evt.PageId, evt.VariantId, label, cancellationToken);
                row.RecordConversion();
                break;
            }

            default:
                return;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static async Task<VariantAnalytics> GetOrCreateAsync(
        OpenDxpDbContext dbContext, Guid pageId, Guid? variantId, string variantLabel, CancellationToken cancellationToken)
    {
        var existing = await dbContext.VariantAnalytics
            .FirstOrDefaultAsync(a => a.PageId == pageId && a.VariantId == variantId, cancellationToken);

        if (existing is not null)
        {
            return existing;
        }

        var created = VariantAnalytics.Create(pageId, variantId, variantLabel);
        dbContext.VariantAnalytics.Add(created);
        return created;
    }
}
