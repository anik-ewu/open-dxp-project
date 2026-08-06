using MediatR;
using OpenDXP.Application.Personalization.Dtos;

namespace OpenDXP.Application.Personalization.Queries;

public record GetVariantAnalyticsQuery(Guid PageId) : IRequest<IReadOnlyList<VariantAnalyticsDto>>;

public class GetVariantAnalyticsQueryHandler(IVariantAnalyticsRepository repository)
    : IRequestHandler<GetVariantAnalyticsQuery, IReadOnlyList<VariantAnalyticsDto>>
{
    public Task<IReadOnlyList<VariantAnalyticsDto>> Handle(GetVariantAnalyticsQuery request, CancellationToken cancellationToken) =>
        repository.GetByPageIdAsync(request.PageId, cancellationToken);
}
