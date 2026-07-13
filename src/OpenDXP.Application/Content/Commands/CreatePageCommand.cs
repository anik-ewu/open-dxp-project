using MediatR;
using OpenDXP.Application.Common.Exceptions;
using OpenDXP.Application.Content.Dtos;
using OpenDXP.Domain.Content;

namespace OpenDXP.Application.Content.Commands;

public record CreatePageCommand(string Slug, string Title, string BlocksJson) : IRequest<PageDetailDto>;

public class CreatePageCommandHandler(IPageRepository repository) : IRequestHandler<CreatePageCommand, PageDetailDto>
{
    public async Task<PageDetailDto> Handle(CreatePageCommand request, CancellationToken cancellationToken)
    {
        if (await repository.SlugExistsAsync(request.Slug, cancellationToken))
        {
            throw new DuplicateSlugException(request.Slug);
        }

        var page = Page.CreateDraft(request.Slug, request.Title, request.BlocksJson);
        repository.Add(page);
        await repository.SaveChangesAsync(cancellationToken);

        return page.ToDetailDto();
    }
}
