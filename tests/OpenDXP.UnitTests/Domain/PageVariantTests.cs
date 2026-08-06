using OpenDXP.Domain.Personalization;

namespace OpenDXP.UnitTests.Domain;

public class PageVariantTests
{
    [Fact]
    public void Create_NormalizesBlankSegmentAndBlocksJson()
    {
        var variant = PageVariant.Create(Guid.NewGuid(), "Mobile", "", "  ", null, 0);

        Assert.Equal("[]", variant.BlocksJson);
        Assert.Null(variant.TargetSegment);
    }

    [Fact]
    public void Create_ThrowsOnMissingName()
    {
        Assert.Throws<ArgumentException>(() => PageVariant.Create(Guid.NewGuid(), "", "[]", "mobile", 50, 0));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(101)]
    public void Create_ThrowsOnTrafficPercentageOutOfRange(int trafficPercentage)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => PageVariant.Create(Guid.NewGuid(), "Variant A", "[]", "mobile", trafficPercentage, 0));
    }
}
