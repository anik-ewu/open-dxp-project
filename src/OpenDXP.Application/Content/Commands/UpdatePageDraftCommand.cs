using MediatR;
using OpenDXP.Application.Common.Exceptions;
using OpenDXP.Application.Content.Dtos;

namespace OpenDXP.Application.Content.Commands;

public record UpdatePageDraftCommand(Guid PageId, string Title, string BlocksJson) : IRequest<PageDetailDto>;

public class UpdatePageDraftCommandHandler(IPageRepository repository)
    : IRequestHandler<UpdatePageDraftCommand, PageDetailDto>
{
    public async Task<PageDetailDto> Handle(UpdatePageDraftCommand request, CancellationToken cancellationToken)
    {
        var page = await repository.GetByIdAsync(request.PageId, cancellationToken)
                   ?? throw new NotFoundException(nameof(Domain.Content.Page), request.PageId);

        page.UpdateDraft(request.Title, request.BlocksJson);
        await repository.SaveChangesAsync(cancellationToken);

        return page.ToDetailDto();
    }
}
