using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenDXP.Application.Common.Auditing;
using OpenDXP.Application.Common.Security;

namespace OpenDXP.Api.Controllers;

[ApiController]
[Route("api/audit-log")]
[Authorize(Roles = Roles.Admin)]
public class AuditLogController(ISender mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AuditLogEntryDto>>> GetRecent(
        [FromQuery] int take, CancellationToken cancellationToken)
    {
        var entries = await mediator.Send(new GetRecentAuditLogQuery(take == 0 ? 100 : take), cancellationToken);
        return Ok(entries);
    }
}
