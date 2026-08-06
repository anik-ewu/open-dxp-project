using MediatR;
using OpenDXP.Application.Common.Exceptions;
using OpenDXP.Application.Content.Dtos;
using OpenDXP.Domain.Content;

namespace OpenDXP.Application.Content.Queries;

public record GetPublishedPageBySlugQuery(string Slug) : IRequest<PublishedPageDto>;

public class GetPublishedPageBySlugQueryHandler(IPageRepository repository)
    : IRequestHandler<GetPublishedPageBySlugQuery, PublishedPageDto>
{
    public async Task<PublishedPageDto> Handle(GetPublishedPageBySlugQuery request, CancellationToken cancellationToken)
    {
        var page = await repository.GetBySlugAsync(request.Slug, cancellationToken);

        if (page is null || page.Status != PageStatus.Published || page.PublishedVersionId is null)
        {
            throw new NotFoundException(nameof(Page), request.Slug);
        }

        var version = page.Versions.Single(v => v.Id == page.PublishedVersionId);

        return new PublishedPageDto(page.Id, page.Slug, version.Title, version.BlocksJson, version.VersionNumber, version.PublishedAt);
    }
}
