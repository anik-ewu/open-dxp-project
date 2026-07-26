using OpenDXP.Domain.Content;

namespace OpenDXP.UnitTests.Domain;

public class PageTests
{
    private static readonly Guid OwnerId = Guid.NewGuid();

    [Fact]
    public void CreateDraft_StartsInDraftStatusWithNoVersions()
    {
        var page = Page.CreateDraft("about-us", "About Us", "[]", OwnerId);

        Assert.Equal(PageStatus.Draft, page.Status);
        Assert.Equal(0, page.LatestVersionNumber);
        Assert.Null(page.PublishedVersionId);
        Assert.Empty(page.Versions);
        Assert.Equal(OwnerId, page.OwnerId);
    }

    [Theory]
    [InlineData("", "Title")]
    [InlineData(" ", "Title")]
    public void CreateDraft_ThrowsOnMissingSlug(string slug, string title)
    {
        Assert.Throws<ArgumentException>(() => Page.CreateDraft(slug, title, "[]", OwnerId));
    }

    [Fact]
    public void CreateDraft_ThrowsOnMissingTitle()
    {
        Assert.Throws<ArgumentException>(() => Page.CreateDraft("about-us", "", "[]", OwnerId));
    }

    [Fact]
    public void UpdateDraft_ChangesTitleAndBlocksWithoutCreatingAVersion()
    {
        var page = Page.CreateDraft("about-us", "About Us", "[]", OwnerId);

        page.UpdateDraft("About Us (v2)", "[{\"type\":\"heading\"}]");

        Assert.Equal("About Us (v2)", page.Title);
        Assert.Equal("[{\"type\":\"heading\"}]", page.BlocksJson);
        Assert.Empty(page.Versions);
        Assert.Equal(PageStatus.Draft, page.Status);
    }

    [Fact]
    public void Publish_SnapshotsDraftIntoFirstVersionAndMarksPagePublished()
    {
        var page = Page.CreateDraft("about-us", "About Us", "[]", OwnerId);

        var version = page.Publish();

        Assert.Equal(1, version.VersionNumber);
        Assert.Equal("About Us", version.Title);
        Assert.Equal(PageStatus.Published, page.Status);
        Assert.Equal(version.Id, page.PublishedVersionId);
        Assert.Single(page.Versions);
    }

    [Fact]
    public void Publish_TwiceAppendsANewVersionInsteadOfOverwritingHistory()
    {
        var page = Page.CreateDraft("about-us", "About Us", "[]", OwnerId);
        var firstVersion = page.Publish();

        page.UpdateDraft("About Us (updated)", "[{\"type\":\"heading\"}]");
        var secondVersion = page.Publish();

        Assert.Equal(2, page.Versions.Count);
        Assert.Equal(1, firstVersion.VersionNumber);
        Assert.Equal(2, secondVersion.VersionNumber);
        Assert.Equal("About Us", firstVersion.Title);
        Assert.Equal("About Us (updated)", secondVersion.Title);
        Assert.Equal(secondVersion.Id, page.PublishedVersionId);
    }

    [Fact]
    public void UpdateDraft_AfterPublishDoesNotMutatePublishedVersionSnapshot()
    {
        var page = Page.CreateDraft("about-us", "About Us", "[]", OwnerId);
        var version = page.Publish();

        page.UpdateDraft("Changed after publish", "[{\"type\":\"paragraph\"}]");

        Assert.Equal("About Us", version.Title);
        Assert.Equal("[]", version.BlocksJson);
    }
}
