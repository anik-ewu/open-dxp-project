using MediatR;
using OpenDXP.Application.Personalization.Dtos;

namespace OpenDXP.Application.Personalization.Queries;

public record GetPageVariantsQuery(Guid PageId) : IRequest<IReadOnlyList<PageVariantDto>>;

public class GetPageVariantsQueryHandler(IPageVariantRepository repository)
    : IRequestHandler<GetPageVariantsQuery, IReadOnlyList<PageVariantDto>>
{
    public async Task<IReadOnlyList<PageVariantDto>> Handle(GetPageVariantsQuery request, CancellationToken cancellationToken)
    {
        var variants = await repository.GetByPageIdAsync(request.PageId, cancellationToken);
        return variants.Select(v => v.ToDto()).ToList();
    }
}
