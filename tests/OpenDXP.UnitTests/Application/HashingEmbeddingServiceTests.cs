using OpenDXP.Application.Search;

namespace OpenDXP.UnitTests.Application;

public class HashingEmbeddingServiceTests
{
    private readonly HashingEmbeddingService sut = new();

    [Fact]
    public void Embed_EmptyText_ReturnsZeroVectorOfCorrectDimension()
    {
        var vector = sut.Embed("");

        Assert.Equal(HashingEmbeddingService.Dimensions, vector.Length);
        Assert.All(vector, v => Assert.Equal(0f, v));
    }

    [Fact]
    public void Embed_SameTextTwice_ProducesIdenticalVectors()
    {
        var first = sut.Embed("Kafka event-driven architecture");
        var second = sut.Embed("Kafka event-driven architecture");

        Assert.Equal(first, second);
    }

    [Fact]
    public void Embed_NonEmptyText_IsL2Normalized()
    {
        var vector = sut.Embed("personalization and experimentation for content delivery");

        var magnitude = MathF.Sqrt(vector.Sum(v => v * v));

        Assert.Equal(1f, magnitude, 3);
    }

    [Fact]
    public void Embed_DifferentText_ProducesDifferentVectors()
    {
        var first = sut.Embed("kubernetes deployment pipeline");
        var second = sut.Embed("audience segmentation targeting");

        Assert.NotEqual(first, second);
    }
}
