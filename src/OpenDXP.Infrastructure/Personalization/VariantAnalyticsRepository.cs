using Microsoft.EntityFrameworkCore;
using OpenDXP.Application.Personalization;
using OpenDXP.Application.Personalization.Dtos;
using OpenDXP.Infrastructure.Persistence;

namespace OpenDXP.Infrastructure.Personalization;

public class VariantAnalyticsRepository(OpenDxpDbContext dbContext) : IVariantAnalyticsRepository
{
    public async Task<IReadOnlyList<VariantAnalyticsDto>> GetByPageIdAsync(Guid pageId, CancellationToken cancellationToken)
    {
        var rows = await dbContext.VariantAnalytics
            .AsNoTracking()
            .Where(a => a.PageId == pageId)
            .ToListAsync(cancellationToken);

        return rows
            .Select(a => new VariantAnalyticsDto(
                a.VariantId,
                a.VariantLabel,
                a.Impressions,
                a.Conversions,
                a.Impressions == 0 ? 0 : Math.Round(100.0 * a.Conversions / a.Impressions, 1)))
            .ToList();
    }
}
