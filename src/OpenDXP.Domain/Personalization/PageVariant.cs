namespace OpenDXP.Domain.Personalization;

/// <summary>
/// An alternate rendering of a page for a targeted audience segment, optionally split further
/// for A/B testing. Evaluated against the page's default (published) content by
/// PersonalizationEngine at delivery time - never mutates or replaces the published content.
/// </summary>
public class PageVariant
{
    public Guid Id { get; private set; }
    public Guid PageId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string BlocksJson { get; private set; } = "[]";

    /// <summary>Null means "untargeted" - eligible for any visitor, used for a plain A/B test.</summary>
    public string? TargetSegment { get; private set; }

    /// <summary>
    /// Null means "always win when eligible." Set alongside sibling variants (that share the same
    /// TargetSegment) to split traffic between them, e.g. two variants at 50 each.
    /// </summary>
    public int? TrafficPercentage { get; private set; }

    /// <summary>Lower evaluates first when multiple variants are eligible without a traffic split.</summary>
    public int Priority { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    private PageVariant()
    {
    }

    public static PageVariant Create(
        Guid pageId, string name, string blocksJson, string? targetSegment, int? trafficPercentage, int priority)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name is required.", nameof(name));
        }

        if (trafficPercentage is < 0 or > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(trafficPercentage), "Must be between 0 and 100.");
        }

        return new PageVariant
        {
            Id = Guid.NewGuid(),
            PageId = pageId,
            Name = name,
            BlocksJson = string.IsNullOrWhiteSpace(blocksJson) ? "[]" : blocksJson,
            TargetSegment = string.IsNullOrWhiteSpace(targetSegment) ? null : targetSegment,
            TrafficPercentage = trafficPercentage,
            Priority = priority,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }
}
