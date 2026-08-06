namespace OpenDXP.Domain.Common;

/// <summary>
/// Base for aggregates that raise domain events. Events are captured into the transactional
/// outbox by DomainEventsToOutboxInterceptor as part of the same SaveChanges call that persists
/// the aggregate, so the business write and the event record commit or roll back together.
/// </summary>
public abstract class AggregateRoot
{
    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void RaiseDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);

    public void ClearDomainEvents() => _domainEvents.Clear();
}
