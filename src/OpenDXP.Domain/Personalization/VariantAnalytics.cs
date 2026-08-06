namespace OpenDXP.Domain.Personalization;

/// <summary>
/// Rolling counters maintained by AnalyticsAggregatorConsumer, one row per variant (plus one
/// synthetic row per page representing the default/control content, VariantId = null).
/// </summary>
public class VariantAnalytics
{
    public Guid Id { get; private set; }
    public Guid PageId { get; private set; }
    public Guid? VariantId { get; private set; }
    public string VariantLabel { get; private set; } = string.Empty;
    public int Impressions { get; private set; }
    public int Conversions { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    private VariantAnalytics()
    {
    }

    public static VariantAnalytics Create(Guid pageId, Guid? variantId, string variantLabel)
    {
        return new VariantAnalytics
        {
            Id = Guid.NewGuid(),
            PageId = pageId,
            VariantId = variantId,
            VariantLabel = variantLabel,
            Impressions = 0,
            Conversions = 0,
            UpdatedAt = DateTimeOffset.UtcNow
        };
    }

    public void RecordImpression()
    {
        Impressions++;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void RecordConversion()
    {
        Conversions++;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
