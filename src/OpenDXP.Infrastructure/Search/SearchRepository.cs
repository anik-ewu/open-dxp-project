using Microsoft.EntityFrameworkCore;
using OpenDXP.Application.Search;
using OpenDXP.Infrastructure.Persistence;

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

    private static string Snippet(string plainText) =>
        plainText.Length <= 200 ? plainText : string.Concat(plainText.AsSpan(0, 200), "…");
}
