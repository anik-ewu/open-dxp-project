using OpenDXP.Application.Tagging;

namespace OpenDXP.UnitTests.Application;

public class KeywordTaggerTests
{
    [Fact]
    public void ExtractTags_EmptyText_ReturnsNoTags()
    {
        Assert.Empty(KeywordTagger.ExtractTags(""));
    }

    [Fact]
    public void ExtractTags_FiltersStopWordsAndShortWords()
    {
        var tags = KeywordTagger.ExtractTags("The quick brown fox jumps over the lazy dog and it is fast");

        Assert.DoesNotContain("the", tags);
        Assert.DoesNotContain("and", tags);
        Assert.DoesNotContain("is", tags);
        Assert.DoesNotContain("it", tags);
    }

    [Fact]
    public void ExtractTags_MostFrequentWordsWin()
    {
        var tags = KeywordTagger.ExtractTags("kafka kafka kafka postgres postgres redis", maxTags: 2);

        Assert.Equal(["kafka", "postgres"], tags);
    }

    [Fact]
    public void ExtractTags_RespectsMaxTags()
    {
        var tags = KeywordTagger.ExtractTags("alpha beta gamma delta epsilon zeta", maxTags: 3);

        Assert.Equal(3, tags.Count);
    }
}
