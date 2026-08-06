using MediatR;
using OpenDXP.Application.Common.Exceptions;
using OpenDXP.Domain.Personalization;

namespace OpenDXP.Application.Personalization.Commands;

public record DeletePageVariantCommand(Guid VariantId) : IRequest;

public class DeletePageVariantCommandHandler(IPageVariantRepository repository) : IRequestHandler<DeletePageVariantCommand>
{
    public async Task Handle(DeletePageVariantCommand request, CancellationToken cancellationToken)
    {
        var variant = await repository.GetByIdAsync(request.VariantId, cancellationToken)
                      ?? throw new NotFoundException(nameof(PageVariant), request.VariantId);

        repository.Remove(variant);
        await repository.SaveChangesAsync(cancellationToken);
    }
}
