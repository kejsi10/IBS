using IBS.BuildingBlocks.Application;
using IBS.BuildingBlocks.Application.Commands;
using IBS.BuildingBlocks.Domain;
using IBS.Documents.Domain.Aggregates.DocumentTemplate;
using IBS.Documents.Domain.Repositories;

namespace IBS.Documents.Application.Commands.CreateDocumentTemplate;

/// <summary>
/// Handler for the CreateDocumentTemplateCommand.
/// </summary>
public sealed class CreateDocumentTemplateCommandHandler(
    IDocumentTemplateRepository templateRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<CreateDocumentTemplateCommand, Guid>
{
    /// <inheritdoc />
    public async Task<Result<Guid>> Handle(CreateDocumentTemplateCommand request, CancellationToken cancellationToken)
    {
        DocumentTemplate template;
        try
        {
            template = DocumentTemplate.Create(
                request.TenantId,
                request.Name,
                request.Description,
                request.TemplateType,
                request.Content,
                request.UserId);
        }
        catch (ArgumentException ex)
        {
            return Error.Validation(ex.Message);
        }

        await templateRepository.AddAsync(template, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return template.Id;
    }
}
