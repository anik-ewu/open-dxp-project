namespace OpenDXP.Application.Common.Auditing;

public record AuditLogEntryDto(Guid Id, string EventType, string? Subject, string Detail, string? IpAddress, DateTimeOffset CreatedAt);
