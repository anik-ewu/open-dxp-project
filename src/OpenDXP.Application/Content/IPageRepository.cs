using OpenDXP.Domain.Content;

namespace OpenDXP.Application.Content;

public interface IPageRepository
{
    Task<Page?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<Page?> GetBySlugAsync(string slug, CancellationToken cancellationToken);

    Task<IReadOnlyList<Page>> GetAllAsync(CancellationToken cancellationToken);

    Task<bool> SlugExistsAsync(string slug, CancellationToken cancellationToken);

    void Add(Page page);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
