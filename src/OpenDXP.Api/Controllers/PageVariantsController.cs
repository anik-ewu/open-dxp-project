using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenDXP.Application.Common.Security;
using OpenDXP.Application.Personalization.Commands;
using OpenDXP.Application.Personalization.Dtos;
using OpenDXP.Application.Personalization.Queries;

namespace OpenDXP.Api.Controllers;

[ApiController]
[Route("api/pages/{pageId:guid}/variants")]
[Authorize]
public class PageVariantsController(ISender mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PageVariantDto>>> GetAll(Guid pageId, CancellationToken cancellationToken)
    {
        var variants = await mediator.Send(new GetPageVariantsQuery(pageId), cancellationToken);
        return Ok(variants);
    }

    [HttpPost]
    [Authorize(Roles = $"{Roles.Editor},{Roles.Admin}")]
    public async Task<ActionResult<PageVariantDto>> Create(
        Guid pageId, CreatePageVariantRequest request, CancellationToken cancellationToken)
    {
        var variant = await mediator.Send(
            new CreatePageVariantCommand(
                pageId, request.Name, request.BlocksJson, request.TargetSegment, request.TrafficPercentage, request.Priority),
            cancellationToken);
        return CreatedAtAction(nameof(GetAll), new { pageId }, variant);
    }

    [HttpDelete("{variantId:guid}")]
    [Authorize(Roles = $"{Roles.Editor},{Roles.Admin}")]
    public async Task<IActionResult> Delete(Guid pageId, Guid variantId, CancellationToken cancellationToken)
    {
        await mediator.Send(new DeletePageVariantCommand(variantId), cancellationToken);
        return NoContent();
    }
}

public record CreatePageVariantRequest(
    string Name, string BlocksJson, string? TargetSegment, int? TrafficPercentage, int Priority);
