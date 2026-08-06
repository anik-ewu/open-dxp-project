using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenDXP.Application.Common.Messaging;
using OpenDXP.Domain.Content;
using OpenDXP.Domain.Search;
using OpenDXP.Infrastructure.Persistence;

namespace OpenDXP.Infrastructure.Messaging.Consumers;

/// <summary>
/// Keeps a denormalized, independently-queryable search index in sync - a read model separate
/// from the Pages table, updated asynchronously rather than on the write path.
/// </summary>
public class SearchReindexConsumer(
    IServiceScopeFactory scopeFactory, IConfiguration configuration, ILogger<SearchReindexConsumer> logger)
    : KafkaConsumerBackgroundService(scopeFactory, configuration, logger, "opendxp-search-reindexer", KafkaTopics.ContentEvents)
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

        var dbContext = scopedProvider.GetRequiredService<OpenDxpDbContext>();
        var plainText = ExtractPlainText(evt.BlocksJson);

        var existing = await dbContext.PageSearchEntries.FindAsync([evt.PageId], cancellationToken);
        if (existing is null)
        {
            dbContext.PageSearchEntries.Add(PageSearchEntry.Create(evt.PageId, evt.Slug, evt.Title, plainText));
        }
        else
        {
            existing.Update(evt.Title, plainText);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static string ExtractPlainText(string blocksJson)
    {
        try
        {
            using var document = JsonDocument.Parse(blocksJson);
            var texts = new List<string>();
            foreach (var block in document.RootElement.EnumerateArray())
            {
                if (block.TryGetProperty("text", out var textProperty) && textProperty.ValueKind == JsonValueKind.String)
                {
                    texts.Add(textProperty.GetString()!);
                }
            }

            return string.Join(" ", texts);
        }
        catch (JsonException)
        {
            return string.Empty;
        }
    }
}
