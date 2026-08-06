using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using OpenDXP.Domain.Common;
using OpenDXP.Domain.Outbox;

namespace OpenDXP.Infrastructure.Outbox;

/// <summary>
/// Captures pending domain events from tracked aggregates into the outbox table as part of the
/// same SaveChanges call that persists the aggregate - the transactional-outbox pattern. This
/// guarantees the business write and the event record commit (or roll back) together, so a
/// crash between "save the Page" and "publish to Kafka" can never lose or fake an event: a
/// separate background publisher (OutboxPublisherService) picks up unprocessed rows afterwards.
/// </summary>
public class DomainEventsToOutboxInterceptor : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        if (eventData.Context is DbContext context)
        {
            AddOutboxMessagesForPendingDomainEvents(context);
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private static void AddOutboxMessagesForPendingDomainEvents(DbContext context)
    {
        var aggregatesWithEvents = context.ChangeTracker.Entries<AggregateRoot>()
            .Select(entry => entry.Entity)
            .Where(aggregate => aggregate.DomainEvents.Count > 0)
            .ToList();

        foreach (var aggregate in aggregatesWithEvents)
        {
            foreach (var domainEvent in aggregate.DomainEvents)
            {
                var message = OutboxMessage.Create(
                    domainEvent.GetType().Name,
                    JsonSerializer.Serialize(domainEvent, domainEvent.GetType()),
                    domainEvent.OccurredAt);

                context.Add(message);
            }

            aggregate.ClearDomainEvents();
        }
    }
}
