using OpenDXP.Application.Personalization;
using OpenDXP.Application.Personalization.Dtos;

namespace OpenDXP.UnitTests.Application;

public class PersonalizationEngineTests
{
    private static readonly Guid PageId = Guid.NewGuid();
    private const string DefaultBlocks = "[\"default\"]";

    private static PageVariantDto Variant(
        string name, string blocks, string? segment = null, int? trafficPercentage = null, int priority = 0) =>
        new(Guid.NewGuid(), PageId, name, blocks, segment, trafficPercentage, priority, DateTimeOffset.UtcNow);

    [Fact]
    public void Select_NoVariants_ReturnsDefault()
    {
        var selection = PersonalizationEngine.Select(PageId, DefaultBlocks, [], visitorId: "v1", segment: null);

        Assert.Null(selection.VariantId);
        Assert.Equal(DefaultBlocks, selection.BlocksJson);
    }

    [Fact]
    public void Select_SegmentMismatch_FallsBackToDefault()
    {
        var variants = new[] { Variant("Mobile Variant", "[\"mobile\"]", segment: "mobile") };

        var selection = PersonalizationEngine.Select(PageId, DefaultBlocks, variants, visitorId: "v1", segment: "desktop");

        Assert.Null(selection.VariantId);
    }

    [Fact]
    public void Select_MatchingSegmentNoSplit_ReturnsThatVariant()
    {
        var variant = Variant("Mobile Variant", "[\"mobile\"]", segment: "mobile");

        var selection = PersonalizationEngine.Select(PageId, DefaultBlocks, [variant], visitorId: "v1", segment: "mobile");

        Assert.Equal(variant.Id, selection.VariantId);
        Assert.Equal("[\"mobile\"]", selection.BlocksJson);
    }

    [Fact]
    public void Select_MultipleMatchesWithoutSplit_LowestPriorityWins()
    {
        var low = Variant("Low priority number", "[\"first\"]", priority: 0);
        var high = Variant("High priority number", "[\"second\"]", priority: 5);

        var selection = PersonalizationEngine.Select(PageId, DefaultBlocks, [high, low], visitorId: "v1", segment: null);

        Assert.Equal(low.Id, selection.VariantId);
    }

    [Fact]
    public void Select_WithTrafficSplit_SameVisitorAlwaysGetsSameBucket()
    {
        var variants = new[] { Variant("A", "[\"a\"]", trafficPercentage: 50), Variant("B", "[\"b\"]", trafficPercentage: 50) };

        var first = PersonalizationEngine.Select(PageId, DefaultBlocks, variants, visitorId: "stable-visitor", segment: null);
        var second = PersonalizationEngine.Select(PageId, DefaultBlocks, variants, visitorId: "stable-visitor", segment: null);

        Assert.Equal(first.VariantId, second.VariantId);
    }

    [Fact]
    public void Select_WithTrafficSplitBelow100_SomeVisitorsFallThroughToDefault()
    {
        var variants = new[] { Variant("A", "[\"a\"]", trafficPercentage: 10) };

        var sawDefault = Enumerable.Range(0, 50)
            .Select(i => PersonalizationEngine.Select(PageId, DefaultBlocks, variants, visitorId: $"visitor-{i}", segment: null))
            .Any(s => s.VariantId is null);

        Assert.True(sawDefault, "Expected at least one of 50 visitors to fall outside a 10% split.");
    }
}
