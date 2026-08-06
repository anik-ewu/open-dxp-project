using OpenDXP.Domain.Personalization;

namespace OpenDXP.Application.Personalization;

public interface IPageVariantRepository
{
    Task<IReadOnlyList<PageVariant>> GetByPageIdAsync(Guid pageId, CancellationToken cancellationToken);

    Task<PageVariant?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    void Add(PageVariant variant);

    void Remove(PageVariant variant);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
