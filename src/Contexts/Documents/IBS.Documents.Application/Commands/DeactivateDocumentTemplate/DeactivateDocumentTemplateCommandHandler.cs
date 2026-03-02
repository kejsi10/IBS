using IBS.BuildingBlocks.Application;
using IBS.BuildingBlocks.Application.Commands;
using IBS.BuildingBlocks.Domain;
using IBS.Documents.Domain.Repositories;

namespace IBS.Documents.Application.Commands.DeactivateDocumentTemplate;

/// <summary>
/// Handler for the DeactivateDocumentTemplateCommand.
/// </summary>
public sealed class DeactivateDocumentTemplateCommandHandler(
    IDocumentTemplateRepository templateRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<DeactivateDocumentTemplateCommand>
{
    /// <inheritdoc />
    public async Task<Result> Handle(DeactivateDocumentTemplateCommand request, CancellationToken cancellationToken)
    {
        var template = await templateRepository.GetByIdAsync(request.TemplateId, cancellationToken);
        if (template is null || template.TenantId != request.TenantId)
            return Error.NotFound("Template not found.");

        try
        {
            template.Deactivate();
        }
        catch (InvalidOperationException ex)
        {
            return Error.Validation(ex.Message);
        }

        await templateRepository.UpdateAsync(template, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
