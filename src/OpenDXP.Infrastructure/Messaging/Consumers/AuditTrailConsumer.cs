using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenDXP.Application.Common.Auditing;
using OpenDXP.Application.Common.Messaging;
using OpenDXP.Domain.Content;

namespace OpenDXP.Infrastructure.Messaging.Consumers;

/// <summary>
/// Writes the publish audit entry from the event rather than inline in PagesController - the
/// controller no longer needs to know an audit trail exists at all. Trade-off: domain events
/// don't carry "who initiated this," so unlike Phase 2's inline audit calls (which had the
/// caller's identity from the HTTP context), this entry has no Subject/actor - a real system
/// would thread an actor id through the event if that mattered for compliance.
/// </summary>
public class AuditTrailConsumer(
    IServiceScopeFactory scopeFactory, IConfiguration configuration, ILogger<AuditTrailConsumer> logger)
    : KafkaConsumerBackgroundService(scopeFactory, configuration, logger, "opendxp-audit-trail", KafkaTopics.ContentEvents)
{
    protected override async Task HandleAsync(
        string eventType, string payloadJson, IServiceProvider scopedProvider, CancellationToken cancellationToken)
    {
        if (eventType != nameof(PagePublishedEvent))
        {
            return;
        }

        var evt = JsonSerializer.Deserialize<PagePublishedEvent>(payloadJson)
                  ?? throw new InvalidOperationException("Could not deserialize PagePublishedEvent.");

        var auditLog = scopedProvider.GetRequiredService<IAuditLogService>();
        await auditLog.LogAsync(
            "PagePublished",
            subject: null,
            detail: $"Page '{evt.Slug}' published as version {evt.VersionNumber}.",
            ipAddress: null,
            cancellationToken);
    }
}
