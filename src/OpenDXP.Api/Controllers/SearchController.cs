using MediatR;
using Microsoft.AspNetCore.Mvc;
using OpenDXP.Application.Search;

namespace OpenDXP.Api.Controllers;

/// <summary>Public search over the Kafka-driven re-index of published content.</summary>
[ApiController]
[Route("api/search")]
public class SearchController(ISender mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<SearchResultDto>>> Search(
        [FromQuery] string q, CancellationToken cancellationToken)
    {
        var results = await mediator.Send(new SearchPagesQuery(q), cancellationToken);
        return Ok(results);
    }
}
