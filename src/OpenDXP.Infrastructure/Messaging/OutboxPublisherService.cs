using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenDXP.Application.Common.Messaging;
using OpenDXP.Infrastructure.Persistence;

namespace OpenDXP.Infrastructure.Messaging;

/// <summary>
/// Drains the transactional outbox to Kafka. Runs independently of the request that wrote the
/// outbox row, so a crash right after SaveChanges just delays the publish to the next poll - it
/// never loses the event (row is already committed) and never publishes one that didn't happen
/// (row only exists if the business write committed).
/// </summary>
public class OutboxPublisherService(
    IServiceScopeFactory scopeFactory,
    IProducer<string, string> producer,
    ILogger<OutboxPublisherService> logger) : BackgroundService
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(2);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await PublishPendingMessagesAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Failed to publish outbox messages; will retry next poll.");
            }

            await Task.Delay(PollInterval, stoppingToken).ContinueWith(_ => { }, TaskScheduler.Default);
        }
    }

    private async Task PublishPendingMessagesAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<OpenDxpDbContext>();

        var pending = await dbContext.OutboxMessages
            .Where(m => m.ProcessedAt == null)
            .OrderBy(m => m.OccurredAt)
            .Take(50)
            .ToListAsync(cancellationToken);

        if (pending.Count == 0)
        {
            return;
        }

        foreach (var message in pending)
        {
            var kafkaMessage = new Message<string, string>
            {
                Key = message.Id.ToString(),
                Value = message.Content,
                Headers = new Headers { { KafkaHeaders.EventType, System.Text.Encoding.UTF8.GetBytes(message.Type) } }
            };

            await producer.ProduceAsync(KafkaTopics.ContentEvents, kafkaMessage, cancellationToken);
            message.MarkProcessed();
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
