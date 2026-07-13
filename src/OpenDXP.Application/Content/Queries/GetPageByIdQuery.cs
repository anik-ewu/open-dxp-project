using MediatR;
using OpenDXP.Application.Common.Exceptions;
using OpenDXP.Application.Content.Dtos;

namespace OpenDXP.Application.Content.Queries;

public record GetPageByIdQuery(Guid PageId) : IRequest<PageDetailDto>;

public class GetPageByIdQueryHandler(IPageRepository repository) : IRequestHandler<GetPageByIdQuery, PageDetailDto>
{
    public async Task<PageDetailDto> Handle(GetPageByIdQuery request, CancellationToken cancellationToken)
    {
        var page = await repository.GetByIdAsync(request.PageId, cancellationToken)
                   ?? throw new NotFoundException(nameof(Domain.Content.Page), request.PageId);

        return page.ToDetailDto();
    }
}
