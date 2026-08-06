namespace OpenDXP.Application.Personalization.Dtos;

public record PageVariantDto(
    Guid Id,
    Guid PageId,
    string Name,
    string BlocksJson,
    string? TargetSegment,
    int? TrafficPercentage,
    int Priority,
    DateTimeOffset CreatedAt);

public record VariantAnalyticsDto(
    Guid? VariantId, string VariantLabel, int Impressions, int Conversions, double ConversionRate);
