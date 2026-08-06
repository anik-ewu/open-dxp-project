namespace OpenDXP.Domain.Search;

/// <summary>
/// Read model kept in sync by the search re-index Kafka consumer, not written directly by the
/// content write path. PageId is the natural key - one entry per page, upserted on every publish.
/// </summary>
public class PageSearchEntry
{
    public Guid PageId { get; private set; }
    public string Slug { get; private set; } = string.Empty;
    public string Title { get; private set; } = string.Empty;
    public string PlainText { get; private set; } = string.Empty;
    public DateTimeOffset UpdatedAt { get; private set; }

    private PageSearchEntry()
    {
    }

    public static PageSearchEntry Create(Guid pageId, string slug, string title, string plainText)
    {
        return new PageSearchEntry
        {
            PageId = pageId,
            Slug = slug,
            Title = title,
            PlainText = plainText,
            UpdatedAt = DateTimeOffset.UtcNow
        };
    }

    public void Update(string title, string plainText)
    {
        Title = title;
        PlainText = plainText;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
