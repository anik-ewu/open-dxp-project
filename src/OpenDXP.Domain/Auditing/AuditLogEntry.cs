namespace OpenDXP.Domain.Auditing;

/// <summary>Append-only trail of security-relevant events (auth, publish, etc).</summary>
public class AuditLogEntry
{
    public Guid Id { get; private set; }
    public string EventType { get; private set; } = string.Empty;
    public string? Subject { get; private set; }
    public string Detail { get; private set; } = string.Empty;
    public string? IpAddress { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    private AuditLogEntry()
    {
    }

    public static AuditLogEntry Create(string eventType, string? subject, string detail, string? ipAddress)
    {
        return new AuditLogEntry
        {
            Id = Guid.NewGuid(),
            EventType = eventType,
            Subject = subject,
            Detail = detail,
            IpAddress = ipAddress,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }
}
