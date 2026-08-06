namespace OpenDXP.Application.Common.Caching;

public static class CacheKeys
{
    public static string PublishedPage(string slug) => $"page:{slug}";
}
