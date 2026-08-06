using System.Text.Json;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using OpenDXP.Application.Common.Caching;
using OpenDXP.Application.Content.Dtos;
using OpenDXP.Application.Content.Queries;
using StackExchange.Redis;

namespace OpenDXP.Api.Controllers;

/// <summary>
/// Public, read-only content delivery API — only ever returns published content.
/// </summary>
[ApiController]
[Route("api/delivery")]
public class DeliveryController(ISender mediator, IConnectionMultiplexer redis) : ControllerBase
{
    [HttpGet("pages/{slug}")]
    public async Task<ActionResult<PublishedPageDto>> GetBySlug(string slug, CancellationToken cancellationToken)
    {
        var db = redis.GetDatabase();
        var cacheKey = CacheKeys.PublishedPage(slug);

        var cached = await db.StringGetAsync(cacheKey);
        if (cached.HasValue)
        {
            return Ok(JsonSerializer.Deserialize<PublishedPageDto>(cached!));
        }

        var page = await mediator.Send(new GetPublishedPageBySlugQuery(slug), cancellationToken);

        // Populate on miss too (not just via the Kafka consumer) so a cold/flushed cache
        // self-heals on the next read instead of staying empty until the next publish.
        await db.StringSetAsync(cacheKey, JsonSerializer.Serialize(page));

        return Ok(page);
    }
}
