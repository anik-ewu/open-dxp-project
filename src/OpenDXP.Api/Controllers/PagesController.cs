using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenDXP.Application.Common.Security;
using OpenDXP.Application.Content.Commands;
using OpenDXP.Application.Content.Dtos;
using OpenDXP.Application.Content.Queries;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace OpenDXP.Api.Controllers;

[ApiController]
[Route("api/pages")]
[Authorize]
public class PagesController(ISender mediator, IAuthorizationService authorizationService) : ControllerBase
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
    [Authorize(Roles = $"{Roles.Editor},{Roles.Admin}")]
    public async Task<ActionResult<PageDetailDto>> Create(CreatePageRequest request, CancellationToken cancellationToken)
    {
        var ownerId = GetCurrentUserId();
        var page = await mediator.Send(
            new CreatePageCommand(request.Slug, request.Title, request.BlocksJson, ownerId), cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = page.Id }, page);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = $"{Roles.Editor},{Roles.Admin}")]
    public async Task<ActionResult<PageDetailDto>> UpdateDraft(
        Guid id, UpdatePageDraftRequest request, CancellationToken cancellationToken)
    {
        var existing = await mediator.Send(new GetPageByIdQuery(id), cancellationToken);
        var authResult = await authorizationService.AuthorizeAsync(User, existing, Policies.MustOwnResource);
        if (!authResult.Succeeded)
        {
            return Forbid();
        }

        var page = await mediator.Send(
            new UpdatePageDraftCommand(id, request.Title, request.BlocksJson), cancellationToken);
        return Ok(page);
    }

    [HttpPost("{id:guid}/publish")]
    [Authorize(Roles = $"{Roles.Editor},{Roles.Admin}")]
    public async Task<ActionResult<PageVersionDto>> Publish(Guid id, CancellationToken cancellationToken)
    {
        var existing = await mediator.Send(new GetPageByIdQuery(id), cancellationToken);
        var authResult = await authorizationService.AuthorizeAsync(User, existing, Policies.MustOwnResource);
        if (!authResult.Succeeded)
        {
            return Forbid();
        }

        var version = await mediator.Send(new PublishPageCommand(id), cancellationToken);
        return Ok(version);
    }

    private Guid GetCurrentUserId()
    {
        var subject = User.FindFirst(Claims.Subject)?.Value;
        return Guid.TryParse(subject, out var userId) ? userId : throw new InvalidOperationException("Missing subject claim.");
    }
}

public record CreatePageRequest(string Slug, string Title, string BlocksJson);

public record UpdatePageDraftRequest(string Title, string BlocksJson);
