using OpenDXP.Application.Personalization.Dtos;

namespace OpenDXP.Application.Personalization;

public interface IVariantAnalyticsRepository
{
    Task<IReadOnlyList<VariantAnalyticsDto>> GetByPageIdAsync(Guid pageId, CancellationToken cancellationToken);
}
