using MediatR;

namespace OpenDXP.Application.Common.Auditing;

public record GetRecentAuditLogQuery(int Take = 100) : IRequest<IReadOnlyList<AuditLogEntryDto>>;
