using System.Text;
using System.Text.Json;
using Confluent.Kafka;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using OpenDXP.Application.Common.Caching;
using OpenDXP.Application.Common.Messaging;
using OpenDXP.Application.Content.Dtos;
using OpenDXP.Application.Content.Queries;
using OpenDXP.Application.Personalization;
using OpenDXP.Application.Personalization.Queries;
using StackExchange.Redis;

namespace OpenDXP.Api.Controllers;

/// <summary>
/// Public, read-only content delivery API — only ever returns published content, personalized
/// per visitor/segment against any active PageVariants.
/// </summary>
[ApiController]
[Route("api/delivery")]
public class DeliveryController(ISender mediator, IConnectionMultiplexer redis, IProducer<string, string> producer)
    : ControllerBase
{
    [HttpGet("pages/{slug}")]
    public async Task<ActionResult<DeliveredPageDto>> GetBySlug(
        string slug, [FromQuery] string? visitorId, [FromQuery] string? segment, CancellationToken cancellationToken)
    {
        var page = await GetPublishedPageAsync(slug, cancellationToken);

        var variants = await mediator.Send(new GetPageVariantsQuery(page.PageId), cancellationToken);
        var selection = PersonalizationEngine.Select(page.PageId, page.BlocksJson, variants, visitorId, segment);

        PublishVariantServed(page.PageId, selection, visitorId, segment);

        return Ok(new DeliveredPageDto(
            page.Slug, page.Title, selection.BlocksJson, page.VersionNumber, page.PublishedAt,
            selection.VariantId, selection.VariantLabel));
    }

    private async Task<PublishedPageDto> GetPublishedPageAsync(string slug, CancellationToken cancellationToken)
    {
        var db = redis.GetDatabase();
        var cacheKey = CacheKeys.PublishedPage(slug);

        var cached = await db.StringGetAsync(cacheKey);
        if (cached.HasValue)
        {
            return JsonSerializer.Deserialize<PublishedPageDto>(cached!)!;
        }

        var page = await mediator.Send(new GetPublishedPageBySlugQuery(slug), cancellationToken);

        // Populate on miss too (not just via the Kafka consumer) so a cold/flushed cache
        // self-heals on the next read instead of staying empty until the next publish.
        await db.StringSetAsync(cacheKey, JsonSerializer.Serialize(page));

        return page;
    }

    private void PublishVariantServed(Guid pageId, VariantSelection selection, string? visitorId, string? segment)
    {
        var evt = new VariantServedEvent(pageId, selection.VariantId, selection.VariantLabel, visitorId, segment, DateTimeOffset.UtcNow);
        var message = new Message<string, string>
        {
            Key = pageId.ToString(),
            Value = JsonSerializer.Serialize(evt),
            Headers = new Headers { { KafkaHeaders.EventType, Encoding.UTF8.GetBytes(nameof(VariantServedEvent)) } }
        };

        // Fire-and-forget: analytics shouldn't add latency or a failure mode to serving content.
        producer.Produce(KafkaTopics.AnalyticsEvents, message);
    }
}

public record DeliveredPageDto(
    string Slug, string Title, string BlocksJson, int VersionNumber, DateTimeOffset PublishedAt,
    Guid? VariantId, string VariantLabel);
