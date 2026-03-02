using IBS.BuildingBlocks.Application;
using IBS.BuildingBlocks.Application.Commands;
using IBS.BuildingBlocks.Domain;
using IBS.Documents.Domain.Repositories;

namespace IBS.Documents.Application.Commands.UpdateDocumentTemplate;

/// <summary>
/// Handler for the UpdateDocumentTemplateCommand.
/// </summary>
public sealed class UpdateDocumentTemplateCommandHandler(
    IDocumentTemplateRepository templateRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<UpdateDocumentTemplateCommand>
{
    /// <inheritdoc />
    public async Task<Result> Handle(UpdateDocumentTemplateCommand request, CancellationToken cancellationToken)
    {
        var template = await templateRepository.GetByIdAsync(request.TemplateId, cancellationToken);
        if (template is null || template.TenantId != request.TenantId)
            return Error.NotFound("Template not found.");

        try
        {
            template.Update(request.Name, request.Description, request.Content);
        }
        catch (ArgumentException ex)
        {
            return Error.Validation(ex.Message);
        }

        await templateRepository.UpdateAsync(template, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
