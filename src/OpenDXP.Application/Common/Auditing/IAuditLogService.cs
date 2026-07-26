namespace OpenDXP.Application.Common.Auditing;

public interface IAuditLogService
{
    Task LogAsync(string eventType, string? subject, string detail, string? ipAddress, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AuditLogEntryDto>> GetRecentAsync(int take, CancellationToken cancellationToken = default);
}
