using MediatR;
using OpenDXP.Application.Common.Exceptions;
using OpenDXP.Domain.Content;

namespace OpenDXP.Application.Content.Commands;

public record SetPageTagsCommand(Guid PageId, IReadOnlyList<string> Tags) : IRequest;

public class SetPageTagsCommandHandler(IPageRepository repository) : IRequestHandler<SetPageTagsCommand>
{
    public async Task Handle(SetPageTagsCommand request, CancellationToken cancellationToken)
    {
        var page = await repository.GetByIdAsync(request.PageId, cancellationToken)
                   ?? throw new NotFoundException(nameof(Page), request.PageId);

        page.SetAutoTags(request.Tags);
        await repository.SaveChangesAsync(cancellationToken);
    }
}
