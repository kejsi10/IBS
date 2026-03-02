using IBS.BuildingBlocks.Application.Commands;

namespace IBS.Documents.Application.Commands.UpdateDocumentTemplate;

/// <summary>
/// Command to update an existing document template.
/// </summary>
public sealed record UpdateDocumentTemplateCommand(
    Guid TenantId,
    Guid TemplateId,
    string Name,
    string Description,
    string Content
) : ICommand;
