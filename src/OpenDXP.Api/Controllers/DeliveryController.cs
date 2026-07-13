using MediatR;
using Microsoft.AspNetCore.Mvc;
using OpenDXP.Application.Content.Dtos;
using OpenDXP.Application.Content.Queries;

namespace OpenDXP.Api.Controllers;

/// <summary>
/// Public, read-only content delivery API — only ever returns published content.
/// </summary>
[ApiController]
[Route("api/delivery")]
public class DeliveryController(ISender mediator) : ControllerBase
{
    [HttpGet("pages/{slug}")]
    public async Task<ActionResult<PublishedPageDto>> GetBySlug(string slug, CancellationToken cancellationToken)
    {
        var page = await mediator.Send(new GetPublishedPageBySlugQuery(slug), cancellationToken);
        return Ok(page);
    }
}
