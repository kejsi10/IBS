using IBS.BuildingBlocks.Application;
using IBS.BuildingBlocks.Application.Commands;
using IBS.BuildingBlocks.Domain;
using IBS.Documents.Domain.Repositories;

namespace IBS.Documents.Application.Commands.ActivateDocumentTemplate;

/// <summary>
/// Handler for the ActivateDocumentTemplateCommand.
/// </summary>
public sealed class ActivateDocumentTemplateCommandHandler(
    IDocumentTemplateRepository templateRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<ActivateDocumentTemplateCommand>
{
    /// <inheritdoc />
    public async Task<Result> Handle(ActivateDocumentTemplateCommand request, CancellationToken cancellationToken)
    {
        var template = await templateRepository.GetByIdAsync(request.TemplateId, cancellationToken);
        if (template is null || template.TenantId != request.TenantId)
            return Error.NotFound("Template not found.");

        try
        {
            template.Activate();
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
