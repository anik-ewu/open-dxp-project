namespace OpenDXP.Domain.Content;

/// <summary>
/// Immutable snapshot of a page taken at publish time. Append-only: never updated or overwritten.
/// </summary>
public class PageVersion
{
    public Guid Id { get; private set; }
    public Guid PageId { get; private set; }
    public int VersionNumber { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string BlocksJson { get; private set; } = "[]";
    public DateTimeOffset PublishedAt { get; private set; }

    private PageVersion()
    {
    }

    internal static PageVersion Create(Guid pageId, int versionNumber, string title, string blocksJson)
    {
        return new PageVersion
        {
            Id = Guid.NewGuid(),
            PageId = pageId,
            VersionNumber = versionNumber,
            Title = title,
            BlocksJson = blocksJson,
            PublishedAt = DateTimeOffset.UtcNow
        };
    }
}
