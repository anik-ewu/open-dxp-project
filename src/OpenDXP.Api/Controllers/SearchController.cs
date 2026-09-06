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

    /// <summary>Cosine-similarity nearest neighbors via pgvector - a real vector query, though the
    /// embeddings themselves are a deterministic hashing placeholder (see HashingEmbeddingService)
    /// rather than a real LLM embedding model.</summary>
    [HttpGet("related/{pageId:guid}")]
    public async Task<ActionResult<IReadOnlyList<SearchResultDto>>> Related(
        Guid pageId, [FromQuery] int take, CancellationToken cancellationToken)
    {
        var results = await mediator.Send(new GetRelatedPagesQuery(pageId, take == 0 ? 5 : take), cancellationToken);
        return Ok(results);
    }
}
