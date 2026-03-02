using IBS.BuildingBlocks.Application.Commands;

namespace IBS.Documents.Application.Commands.DeactivateDocumentTemplate;

/// <summary>
/// Command to deactivate a document template.
/// </summary>
public sealed record DeactivateDocumentTemplateCommand(
    Guid TenantId,
    Guid TemplateId
) : ICommand;
