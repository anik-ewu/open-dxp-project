using MediatR;
using OpenDXP.Application.Personalization.Dtos;
using OpenDXP.Domain.Personalization;

namespace OpenDXP.Application.Personalization.Commands;

public record CreatePageVariantCommand(
    Guid PageId, string Name, string BlocksJson, string? TargetSegment, int? TrafficPercentage, int Priority)
    : IRequest<PageVariantDto>;

public class CreatePageVariantCommandHandler(IPageVariantRepository repository)
    : IRequestHandler<CreatePageVariantCommand, PageVariantDto>
{
    public async Task<PageVariantDto> Handle(CreatePageVariantCommand request, CancellationToken cancellationToken)
    {
        var variant = PageVariant.Create(
            request.PageId, request.Name, request.BlocksJson, request.TargetSegment, request.TrafficPercentage, request.Priority);

        repository.Add(variant);
        await repository.SaveChangesAsync(cancellationToken);

        return variant.ToDto();
    }
}
