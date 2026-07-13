using MediatR;
using OpenDXP.Application.Content.Dtos;

namespace OpenDXP.Application.Content.Queries;

public record GetPagesQuery : IRequest<IReadOnlyList<PageSummaryDto>>;

public class GetPagesQueryHandler(IPageRepository repository)
    : IRequestHandler<GetPagesQuery, IReadOnlyList<PageSummaryDto>>
{
    public async Task<IReadOnlyList<PageSummaryDto>> Handle(GetPagesQuery request, CancellationToken cancellationToken)
    {
        var pages = await repository.GetAllAsync(cancellationToken);
        return pages.Select(p => p.ToSummaryDto()).ToList();
    }
}
