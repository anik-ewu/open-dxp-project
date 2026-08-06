using OpenDXP.Application.Personalization.Dtos;
using OpenDXP.Domain.Personalization;

namespace OpenDXP.Application.Personalization;

public static class PageVariantMappingExtensions
{
    public static PageVariantDto ToDto(this PageVariant variant) => new(
        variant.Id, variant.PageId, variant.Name, variant.BlocksJson,
        variant.TargetSegment, variant.TrafficPercentage, variant.Priority, variant.CreatedAt);
}
