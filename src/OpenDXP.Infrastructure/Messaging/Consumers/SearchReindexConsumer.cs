using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;
using OpenDXP.Application.Common.Messaging;
using OpenDXP.Application.Content;
using OpenDXP.Application.Search;
using OpenDXP.Domain.Content;
using OpenDXP.Domain.Search;
using OpenDXP.Infrastructure.Persistence;
using Pgvector;

namespace OpenDXP.Infrastructure.Messaging.Consumers;

/// <summary>
/// Keeps a denormalized, independently-queryable search index in sync - a read model separate
/// from the Pages table, updated asynchronously rather than on the write path. Also computes the
/// embedding used by the "related pages" pgvector query.
/// </summary>
public class SearchReindexConsumer(
    IServiceScopeFactory scopeFactory,
    IConfiguration configuration,
    ILogger<SearchReindexConsumer> logger,
    IEmbeddingService embeddingService)
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
        var plainText = BlockTextExtractor.ExtractPlainText(evt.BlocksJson);
        var embedding = embeddingService.Embed($"{evt.Title} {plainText}");

        var existing = await dbContext.PageSearchEntries.FindAsync([evt.PageId], cancellationToken);
        if (existing is null)
        {
            dbContext.PageSearchEntries.Add(PageSearchEntry.Create(evt.PageId, evt.Slug, evt.Title, plainText, embedding));
        }
        else
        {
            existing.Update(evt.Title, plainText, embedding);
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        // Embedding is excluded from the EF model (see PageSearchEntryConfiguration). Raw
        // NpgsqlCommand, not EF's ExecuteSqlInterpolatedAsync - the latter resolves parameter
        // types via EF's own type mapping source, which doesn't know Vector either; only Npgsql's
        // ADO-level type inference (configured via UseVector() on the data source) does.
        var connection = (NpgsqlConnection)dbContext.Database.GetDbConnection();
        var wasClosed = connection.State != System.Data.ConnectionState.Open;
        if (wasClosed)
        {
            await connection.OpenAsync(cancellationToken);
        }

        try
        {
            await using var command = new NpgsqlCommand(
                "UPDATE \"PageSearchEntries\" SET \"Embedding\" = @embedding WHERE \"PageId\" = @pageId", connection);
            command.Parameters.AddWithValue("embedding", new Vector(embedding));
            command.Parameters.AddWithValue("pageId", evt.PageId);
            await command.ExecuteNonQueryAsync(cancellationToken);
        }
        finally
        {
            if (wasClosed)
            {
                await connection.CloseAsync();
            }
        }
    }
}
