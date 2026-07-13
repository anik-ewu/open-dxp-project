using Microsoft.EntityFrameworkCore;
using OpenDXP.Application.Content;
using OpenDXP.Domain.Content;

namespace OpenDXP.Infrastructure.Persistence;

public class PageRepository(OpenDxpDbContext dbContext) : IPageRepository
{
    public Task<Page?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.Pages.Include(p => p.Versions).FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public Task<Page?> GetBySlugAsync(string slug, CancellationToken cancellationToken) =>
        dbContext.Pages.Include(p => p.Versions).FirstOrDefaultAsync(p => p.Slug == slug, cancellationToken);

    public async Task<IReadOnlyList<Page>> GetAllAsync(CancellationToken cancellationToken) =>
        await dbContext.Pages.AsNoTracking().OrderByDescending(p => p.UpdatedAt).ToListAsync(cancellationToken);

    public Task<bool> SlugExistsAsync(string slug, CancellationToken cancellationToken) =>
        dbContext.Pages.AnyAsync(p => p.Slug == slug, cancellationToken);

    public void Add(Page page) => dbContext.Pages.Add(page);

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
