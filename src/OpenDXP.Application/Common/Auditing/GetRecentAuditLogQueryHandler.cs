using MediatR;

namespace OpenDXP.Application.Common.Auditing;

public class GetRecentAuditLogQueryHandler(IAuditLogService auditLogService)
    : IRequestHandler<GetRecentAuditLogQuery, IReadOnlyList<AuditLogEntryDto>>
{
    public Task<IReadOnlyList<AuditLogEntryDto>> Handle(GetRecentAuditLogQuery request, CancellationToken cancellationToken) =>
        auditLogService.GetRecentAsync(request.Take, cancellationToken);
}
