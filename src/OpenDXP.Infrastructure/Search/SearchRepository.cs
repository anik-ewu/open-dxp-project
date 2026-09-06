using System.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using OpenDXP.Application.Search;
using OpenDXP.Infrastructure.Persistence;
using Pgvector;

namespace OpenDXP.Infrastructure.Search;

public class SearchRepository(OpenDxpDbContext dbContext) : ISearchRepository
{
    public async Task<IReadOnlyList<SearchResultDto>> SearchAsync(string query, CancellationToken cancellationToken)
    {
        var pattern = $"%{query}%";

        var matches = await dbContext.PageSearchEntries
            .AsNoTracking()
            .Where(e => EF.Functions.ILike(e.Title, pattern) || EF.Functions.ILike(e.PlainText, pattern))
            .OrderByDescending(e => e.UpdatedAt)
            .Take(20)
            .ToListAsync(cancellationToken);

        return matches
            .Select(e => new SearchResultDto(e.PageId, e.Slug, e.Title, Snippet(e.PlainText)))
            .ToList();
    }

    /// <summary>
    /// Raw ADO.NET, not LINQ: pgvector's cosine distance operator (&lt;=&gt;) and the Vector
    /// parameter type aren't translatable by EF Core without the EF Core 9-only plugin (see
    /// PageSearchEntryConfiguration). Npgsql's own vector type handling (UseVector() on the data
    /// source) still applies at this ADO level.
    /// </summary>
    public async Task<IReadOnlyList<SearchResultDto>> GetRelatedAsync(Guid pageId, int take, CancellationToken cancellationToken)
    {
        var connection = (NpgsqlConnection)dbContext.Database.GetDbConnection();
        var wasClosed = connection.State != ConnectionState.Open;
        if (wasClosed)
        {
            await connection.OpenAsync(cancellationToken);
        }

        try
        {
            var sourceEmbedding = await GetEmbeddingAsync(connection, pageId, cancellationToken);
            if (sourceEmbedding is null)
            {
                return [];
            }

            var results = new List<SearchResultDto>();

            await using var command = new NpgsqlCommand(
                """
                SELECT "PageId", "Slug", "Title", left("PlainText", 200)
                FROM "PageSearchEntries"
                WHERE "PageId" != @pageId AND "Embedding" IS NOT NULL
                ORDER BY "Embedding" <=> @source
                LIMIT @take
                """,
                connection);
            command.Parameters.AddWithValue("pageId", pageId);
            command.Parameters.AddWithValue("source", sourceEmbedding);
            command.Parameters.AddWithValue("take", take);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                results.Add(new SearchResultDto(reader.GetGuid(0), reader.GetString(1), reader.GetString(2), reader.GetString(3)));
            }

            return results;
        }
        finally
        {
            if (wasClosed)
            {
                await connection.CloseAsync();
            }
        }
    }

    private static async Task<Vector?> GetEmbeddingAsync(NpgsqlConnection connection, Guid pageId, CancellationToken cancellationToken)
    {
        await using var command = new NpgsqlCommand(
            "SELECT \"Embedding\" FROM \"PageSearchEntries\" WHERE \"PageId\" = @pageId", connection);
        command.Parameters.AddWithValue("pageId", pageId);

        // ExecuteScalarAsync + `as Vector` fails under Npgsql's newer type-info pipeline (it can't
        // infer a plugin type like Vector when asked for a generic object) - GetFieldValue<Vector>
        // requests the type explicitly.
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken) || await reader.IsDBNullAsync(0, cancellationToken))
        {
            return null;
        }

        return reader.GetFieldValue<Vector>(0);
    }

    private static string Snippet(string plainText) =>
        plainText.Length <= 200 ? plainText : string.Concat(plainText.AsSpan(0, 200), "…");
}
