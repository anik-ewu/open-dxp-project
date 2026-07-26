using Microsoft.EntityFrameworkCore;
using OpenDXP.Application.Common.Auditing;
using OpenDXP.Domain.Auditing;
using OpenDXP.Infrastructure.Persistence;

namespace OpenDXP.Infrastructure.Auditing;

public class AuditLogService(OpenDxpDbContext dbContext) : IAuditLogService
{
    public async Task LogAsync(string eventType, string? subject, string detail, string? ipAddress, CancellationToken cancellationToken = default)
    {
        dbContext.AuditLogEntries.Add(AuditLogEntry.Create(eventType, subject, detail, ipAddress));
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AuditLogEntryDto>> GetRecentAsync(int take, CancellationToken cancellationToken = default) =>
        await dbContext.AuditLogEntries
            .AsNoTracking()
            .OrderByDescending(e => e.CreatedAt)
            .Take(take)
            .Select(e => new AuditLogEntryDto(e.Id, e.EventType, e.Subject, e.Detail, e.IpAddress, e.CreatedAt))
            .ToListAsync(cancellationToken);
}
