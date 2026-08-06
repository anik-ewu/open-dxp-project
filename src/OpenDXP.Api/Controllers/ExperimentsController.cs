using System.Text;
using System.Text.Json;
using Confluent.Kafka;
using Microsoft.AspNetCore.Mvc;
using OpenDXP.Application.Common.Messaging;

namespace OpenDXP.Api.Controllers;

/// <summary>Public endpoint for the delivered site to report a conversion (click, signup, etc).</summary>
[ApiController]
[Route("api/experiments")]
public class ExperimentsController(IProducer<string, string> producer) : ControllerBase
{
    [HttpPost("conversions")]
    public IActionResult RecordConversion(RecordConversionRequest request)
    {
        var evt = new ConversionRecordedEvent(request.PageId, request.VariantId, request.VisitorId, DateTimeOffset.UtcNow);
        var message = new Message<string, string>
        {
            Key = request.PageId.ToString(),
            Value = JsonSerializer.Serialize(evt),
            Headers = new Headers { { KafkaHeaders.EventType, Encoding.UTF8.GetBytes(nameof(ConversionRecordedEvent)) } }
        };

        producer.Produce(KafkaTopics.AnalyticsEvents, message);

        return Accepted();
    }
}

public record RecordConversionRequest(Guid PageId, Guid? VariantId, string? VisitorId);
