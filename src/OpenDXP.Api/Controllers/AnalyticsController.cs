using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenDXP.Application.Personalization.Dtos;
using OpenDXP.Application.Personalization.Queries;

namespace OpenDXP.Api.Controllers;

[ApiController]
[Route("api/analytics")]
[Authorize]
public class AnalyticsController(ISender mediator) : ControllerBase
{
    [HttpGet("pages/{pageId:guid}/variants")]
    public async Task<ActionResult<IReadOnlyList<VariantAnalyticsDto>>> GetVariantAnalytics(
        Guid pageId, CancellationToken cancellationToken)
    {
        var stats = await mediator.Send(new GetVariantAnalyticsQuery(pageId), cancellationToken);
        return Ok(stats);
    }
}
