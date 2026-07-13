namespace OpenDXP.Application.Content.Dtos;

public record PageSummaryDto(Guid Id, string Slug, string Title, string Status, DateTimeOffset UpdatedAt);

public record PageVersionDto(Guid Id, int VersionNumber, string Title, DateTimeOffset PublishedAt);

public record PageDetailDto(
    Guid Id,
    string Slug,
    string Title,
    string BlocksJson,
    string Status,
    int LatestVersionNumber,
    IReadOnlyList<PageVersionDto> Versions);

public record PublishedPageDto(string Slug, string Title, string BlocksJson, int VersionNumber, DateTimeOffset PublishedAt);
