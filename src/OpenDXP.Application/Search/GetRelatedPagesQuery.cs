using MediatR;

namespace OpenDXP.Application.Search;

public record GetRelatedPagesQuery(Guid PageId, int Take = 5) : IRequest<IReadOnlyList<SearchResultDto>>;

public class GetRelatedPagesQueryHandler(ISearchRepository repository)
    : IRequestHandler<GetRelatedPagesQuery, IReadOnlyList<SearchResultDto>>
{
    public Task<IReadOnlyList<SearchResultDto>> Handle(GetRelatedPagesQuery request, CancellationToken cancellationToken) =>
        repository.GetRelatedAsync(request.PageId, request.Take, cancellationToken);
}
