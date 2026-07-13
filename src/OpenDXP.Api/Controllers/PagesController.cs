using MediatR;
using Microsoft.AspNetCore.Mvc;
using OpenDXP.Application.Content.Commands;
using OpenDXP.Application.Content.Dtos;
using OpenDXP.Application.Content.Queries;

namespace OpenDXP.Api.Controllers;

[ApiController]
[Route("api/pages")]
public class PagesController(ISender mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PageSummaryDto>>> GetAll(CancellationToken cancellationToken)
    {
        var pages = await mediator.Send(new GetPagesQuery(), cancellationToken);
        return Ok(pages);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PageDetailDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var page = await mediator.Send(new GetPageByIdQuery(id), cancellationToken);
        return Ok(page);
    }

    [HttpPost]
    public async Task<ActionResult<PageDetailDto>> Create(CreatePageRequest request, CancellationToken cancellationToken)
    {
        var page = await mediator.Send(
            new CreatePageCommand(request.Slug, request.Title, request.BlocksJson), cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = page.Id }, page);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<PageDetailDto>> UpdateDraft(
        Guid id, UpdatePageDraftRequest request, CancellationToken cancellationToken)
    {
        var page = await mediator.Send(
            new UpdatePageDraftCommand(id, request.Title, request.BlocksJson), cancellationToken);
        return Ok(page);
    }

    [HttpPost("{id:guid}/publish")]
    public async Task<ActionResult<PageVersionDto>> Publish(Guid id, CancellationToken cancellationToken)
    {
        var version = await mediator.Send(new PublishPageCommand(id), cancellationToken);
        return Ok(version);
    }
}

public record CreatePageRequest(string Slug, string Title, string BlocksJson);

public record UpdatePageDraftRequest(string Title, string BlocksJson);
