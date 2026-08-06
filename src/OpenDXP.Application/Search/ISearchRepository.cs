namespace OpenDXP.Application.Search;

public interface ISearchRepository
{
    Task<IReadOnlyList<SearchResultDto>> SearchAsync(string query, CancellationToken cancellationToken);
}
