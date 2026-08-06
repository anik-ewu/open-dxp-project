using OpenDXP.Domain.Common;

namespace OpenDXP.Domain.Content;

/// <summary>
/// Aggregate root. Title/BlocksJson hold the current draft; publishing snapshots them into
/// an append-only PageVersion rather than overwriting history.
/// </summary>
public class Page : AggregateRoot
{
    public Guid Id { get; private set; }
    public string Slug { get; private set; } = string.Empty;
    public string Title { get; private set; } = string.Empty;
    public string BlocksJson { get; private set; } = "[]";
    public PageStatus Status { get; private set; }
    public int LatestVersionNumber { get; private set; }
    public Guid? PublishedVersionId { get; private set; }
    public Guid OwnerId { get; private set; }
    public string TagsCsv { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    /// <summary>Set asynchronously by AutoTaggingConsumer after publish - not part of the draft/publish flow.</summary>
    public IReadOnlyList<string> Tags => TagsCsv.Length == 0 ? [] : TagsCsv.Split(',', StringSplitOptions.RemoveEmptyEntries);

    private readonly List<PageVersion> _versions = new();
    public IReadOnlyCollection<PageVersion> Versions => _versions.AsReadOnly();

    private Page()
    {
    }

    public static Page CreateDraft(string slug, string title, string blocksJson, Guid ownerId)
    {
        if (string.IsNullOrWhiteSpace(slug))
        {
            throw new ArgumentException("Slug is required.", nameof(slug));
        }

        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Title is required.", nameof(title));
        }

        var now = DateTimeOffset.UtcNow;
        return new Page
        {
            Id = Guid.NewGuid(),
            Slug = slug,
            Title = title,
            BlocksJson = string.IsNullOrWhiteSpace(blocksJson) ? "[]" : blocksJson,
            Status = PageStatus.Draft,
            LatestVersionNumber = 0,
            OwnerId = ownerId,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    public void UpdateDraft(string title, string blocksJson)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Title is required.", nameof(title));
        }

        Title = title;
        BlocksJson = string.IsNullOrWhiteSpace(blocksJson) ? "[]" : blocksJson;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public PageVersion Publish()
    {
        LatestVersionNumber++;
        var version = PageVersion.Create(Id, LatestVersionNumber, Title, BlocksJson);
        _versions.Add(version);
        PublishedVersionId = version.Id;
        Status = PageStatus.Published;
        UpdatedAt = DateTimeOffset.UtcNow;

        RaiseDomainEvent(new PagePublishedEvent(Id, Slug, Title, BlocksJson, version.VersionNumber, UpdatedAt));

        return version;
    }

    /// <summary>Doesn't touch UpdatedAt - tagging is metadata enrichment, not a content edit.</summary>
    public void SetAutoTags(IEnumerable<string> tags) => TagsCsv = string.Join(',', tags);
}
