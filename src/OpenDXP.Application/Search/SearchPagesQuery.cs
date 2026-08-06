using MediatR;

namespace OpenDXP.Application.Search;

public record SearchPagesQuery(string Query) : IRequest<IReadOnlyList<SearchResultDto>>;

public class SearchPagesQueryHandler(ISearchRepository repository)
    : IRequestHandler<SearchPagesQuery, IReadOnlyList<SearchResultDto>>
{
    public Task<IReadOnlyList<SearchResultDto>> Handle(SearchPagesQuery request, CancellationToken cancellationToken) =>
        string.IsNullOrWhiteSpace(request.Query)
            ? Task.FromResult<IReadOnlyList<SearchResultDto>>([])
            : repository.SearchAsync(request.Query.Trim(), cancellationToken);
}
