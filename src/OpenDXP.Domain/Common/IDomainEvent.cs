namespace OpenDXP.Domain.Common;

public interface IDomainEvent
{
    DateTimeOffset OccurredAt { get; }
}
