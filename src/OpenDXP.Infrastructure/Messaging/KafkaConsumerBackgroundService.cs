using System.Text;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenDXP.Application.Common.Messaging;
using OpenDXP.Application.Common.Telemetry;

namespace OpenDXP.Infrastructure.Messaging;

/// <summary>
/// Each subclass uses its own consumer group id, so multiple consumers can independently process
/// every message on the same topic (Kafka fan-out via consumer groups) - cache invalidation,
/// search re-indexing, and audit logging all react to the same PagePublished event without
/// knowing about each other.
/// </summary>
public abstract class KafkaConsumerBackgroundService(
    IServiceScopeFactory scopeFactory,
    IConfiguration configuration,
    ILogger logger,
    string groupId,
    string topic) : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken) =>
        Task.Run(() => ConsumeLoop(stoppingToken), stoppingToken);

    private void ConsumeLoop(CancellationToken stoppingToken)
    {
        var bootstrapServers = configuration["Kafka:BootstrapServers"] ?? "localhost:19092";
        var config = new ConsumerConfig
        {
            BootstrapServers = bootstrapServers,
            GroupId = groupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false
        };

        using var consumer = new ConsumerBuilder<string, string>(config).Build();
        consumer.Subscribe(topic);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var result = consumer.Consume(stoppingToken);
                    if (result?.Message is null)
                    {
                        continue;
                    }

                    var eventType = result.Message.Headers.TryGetLastBytes(KafkaHeaders.EventType, out var bytes)
                        ? Encoding.UTF8.GetString(bytes)
                        : string.Empty;

                    using var activity = OpenDxpTelemetry.ActivitySource.StartActivity($"Consume {eventType}");
                    activity?.SetTag("messaging.system", "kafka");
                    activity?.SetTag("messaging.destination", topic);
                    activity?.SetTag("messaging.consumer_group", groupId);

                    var groupTag = new KeyValuePair<string, object?>("consumer_group", groupId);

                    using var scope = scopeFactory.CreateScope();
                    HandleAsync(eventType, result.Message.Value, scope.ServiceProvider, stoppingToken)
                        .GetAwaiter().GetResult();

                    consumer.Commit(result);
                    OpenDxpTelemetry.KafkaMessagesProcessed.Add(1, groupTag);
                }
                catch (ConsumeException ex)
                {
                    logger.LogError(ex, "Kafka consume error in consumer group {GroupId}.", groupId);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    logger.LogError(ex, "Error handling message in consumer group {GroupId}.", groupId);
                    OpenDxpTelemetry.KafkaMessagesFailed.Add(1, new KeyValuePair<string, object?>("consumer_group", groupId));
                }
            }
        }
        catch (OperationCanceledException)
        {
            // expected on shutdown
        }
        finally
        {
            consumer.Close();
        }
    }

    protected abstract Task HandleAsync(
        string eventType, string payloadJson, IServiceProvider scopedProvider, CancellationToken cancellationToken);
}
