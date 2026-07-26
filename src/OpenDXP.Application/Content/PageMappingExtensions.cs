using OpenDXP.Application.Content.Dtos;
using OpenDXP.Domain.Content;

namespace OpenDXP.Application.Content;

public static class PageMappingExtensions
{
    public static PageSummaryDto ToSummaryDto(this Page page) =>
        new(page.Id, page.Slug, page.Title, page.Status.ToString(), page.UpdatedAt);

    public static PageVersionDto ToDto(this PageVersion version) =>
        new(version.Id, version.VersionNumber, version.Title, version.PublishedAt);

    public static PageDetailDto ToDetailDto(this Page page) =>
        new(
            page.Id,
            page.Slug,
            page.Title,
            page.BlocksJson,
            page.Status.ToString(),
            page.LatestVersionNumber,
            page.OwnerId,
            page.Versions.OrderByDescending(v => v.VersionNumber).Select(v => v.ToDto()).ToList());
}
