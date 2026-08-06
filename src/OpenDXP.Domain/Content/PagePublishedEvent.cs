using OpenDXP.Domain.Common;

namespace OpenDXP.Domain.Content;

public record PagePublishedEvent(
    Guid PageId,
    string Slug,
    string Title,
    string BlocksJson,
    int VersionNumber,
    DateTimeOffset OccurredAt) : IDomainEvent;
