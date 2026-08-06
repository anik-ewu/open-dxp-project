using Microsoft.EntityFrameworkCore;
using OpenDXP.Application.Personalization;
using OpenDXP.Domain.Personalization;
using OpenDXP.Infrastructure.Persistence;

namespace OpenDXP.Infrastructure.Personalization;

public class PageVariantRepository(OpenDxpDbContext dbContext) : IPageVariantRepository
{
    public async Task<IReadOnlyList<PageVariant>> GetByPageIdAsync(Guid pageId, CancellationToken cancellationToken) =>
        await dbContext.PageVariants
            .Where(v => v.PageId == pageId)
            .OrderBy(v => v.Priority)
            .ToListAsync(cancellationToken);

    public Task<PageVariant?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.PageVariants.FirstOrDefaultAsync(v => v.Id == id, cancellationToken);

    public void Add(PageVariant variant) => dbContext.PageVariants.Add(variant);

    public void Remove(PageVariant variant) => dbContext.PageVariants.Remove(variant);

    public Task SaveChangesAsync(CancellationToken cancellationToken) => dbContext.SaveChangesAsync(cancellationToken);
}
