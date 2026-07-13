using MediatR;
using OpenDXP.Application.Common.Exceptions;
using OpenDXP.Application.Content.Dtos;

namespace OpenDXP.Application.Content.Commands;

public record PublishPageCommand(Guid PageId) : IRequest<PageVersionDto>;

public class PublishPageCommandHandler(IPageRepository repository) : IRequestHandler<PublishPageCommand, PageVersionDto>
{
    public async Task<PageVersionDto> Handle(PublishPageCommand request, CancellationToken cancellationToken)
    {
        var page = await repository.GetByIdAsync(request.PageId, cancellationToken)
                   ?? throw new NotFoundException(nameof(Domain.Content.Page), request.PageId);

        var version = page.Publish();
        await repository.SaveChangesAsync(cancellationToken);

        return version.ToDto();
    }
}
