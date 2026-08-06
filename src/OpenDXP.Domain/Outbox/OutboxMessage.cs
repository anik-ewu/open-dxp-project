namespace OpenDXP.Domain.Outbox;

/// <summary>Append-only outbox row, written atomically with the aggregate that raised the event.</summary>
public class OutboxMessage
{
    public Guid Id { get; private set; }
    public string Type { get; private set; } = string.Empty;
    public string Content { get; private set; } = string.Empty;
    public DateTimeOffset OccurredAt { get; private set; }
    public DateTimeOffset? ProcessedAt { get; private set; }

    private OutboxMessage()
    {
    }

    public static OutboxMessage Create(string type, string content, DateTimeOffset occurredAt)
    {
        return new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Type = type,
            Content = content,
            OccurredAt = occurredAt
        };
    }

    public void MarkProcessed() => ProcessedAt = DateTimeOffset.UtcNow;
}
