using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace OpenDXP.Application.Common.Telemetry;

/// <summary>
/// Domain-level tracing/metrics, separate from the framework instrumentation (ASP.NET Core,
/// HttpClient, Npgsql) wired in Program.cs. Both are System.Diagnostics types from the BCL, so
/// Application stays free of any OpenTelemetry package reference - only the Api host needs to
/// know an exporter exists at all.
/// </summary>
public static class OpenDxpTelemetry
{
    public const string SourceName = "OpenDXP";

    public static readonly ActivitySource ActivitySource = new(SourceName);
    private static readonly Meter Meter = new(SourceName);

    public static readonly Counter<long> PagesPublished =
        Meter.CreateCounter<long>("opendxp.pages.published", description: "Pages published, by version.");

    public static readonly Counter<long> KafkaMessagesProcessed =
        Meter.CreateCounter<long>("opendxp.kafka.messages_processed", description: "Kafka messages successfully handled, by consumer group.");

    public static readonly Counter<long> KafkaMessagesFailed =
        Meter.CreateCounter<long>("opendxp.kafka.messages_failed", description: "Kafka messages that threw during handling, by consumer group.");
}
