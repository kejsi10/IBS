using IBS.BuildingBlocks.Application.Commands;
using IBS.Documents.Domain.Aggregates.DocumentTemplate;

namespace IBS.Documents.Application.Commands.CreateDocumentTemplate;

/// <summary>
/// Command to create a new document template.
/// </summary>
public sealed record CreateDocumentTemplateCommand(
    Guid TenantId,
    string UserId,
    string Name,
    string Description,
    TemplateType TemplateType,
    string Content
) : ICommand<Guid>;
