using IBS.BuildingBlocks.Application.Commands;

namespace IBS.Documents.Application.Commands.ActivateDocumentTemplate;

/// <summary>
/// Command to activate a document template.
/// </summary>
public sealed record ActivateDocumentTemplateCommand(
    Guid TenantId,
    Guid TemplateId
) : ICommand;
