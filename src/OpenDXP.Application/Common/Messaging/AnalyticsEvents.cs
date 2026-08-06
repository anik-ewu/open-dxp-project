namespace OpenDXP.Application.Common.Messaging;

/// <summary>
/// Integration events, not domain events: fired directly from the read path (serving a page,
/// recording a click) rather than raised by an aggregate and captured via the transactional
/// outbox. Losing an occasional impression/conversion is an acceptable trade-off for not adding
/// write-path latency or DB load to every page view - unlike PagePublishedEvent, which must never
/// desync from the actual content state.
/// </summary>
public record VariantServedEvent(
    Guid PageId, Guid? VariantId, string VariantLabel, string? VisitorId, string? Segment, DateTimeOffset OccurredAt);

public record ConversionRecordedEvent(Guid PageId, Guid? VariantId, string? VisitorId, DateTimeOffset OccurredAt);
