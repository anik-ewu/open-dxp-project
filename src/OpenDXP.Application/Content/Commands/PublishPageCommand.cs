using MediatR;
using OpenDXP.Application.Common.Exceptions;
using OpenDXP.Application.Common.Telemetry;
using OpenDXP.Application.Content.Dtos;

namespace OpenDXP.Application.Content.Commands;

public record PublishPageCommand(Guid PageId) : IRequest<PageVersionDto>;

public class PublishPageCommandHandler(IPageRepository repository) : IRequestHandler<PublishPageCommand, PageVersionDto>
{
    public async Task<PageVersionDto> Handle(PublishPageCommand request, CancellationToken cancellationToken)
    {
        using var activity = OpenDxpTelemetry.ActivitySource.StartActivity("PublishPage");
        activity?.SetTag("page.id", request.PageId);

        var page = await repository.GetByIdAsync(request.PageId, cancellationToken)
                   ?? throw new NotFoundException(nameof(Domain.Content.Page), request.PageId);

        var version = page.Publish();
        await repository.SaveChangesAsync(cancellationToken);

        activity?.SetTag("page.version", version.VersionNumber);
        OpenDxpTelemetry.PagesPublished.Add(1, new KeyValuePair<string, object?>("version", version.VersionNumber));

        return version.ToDto();
    }
}
