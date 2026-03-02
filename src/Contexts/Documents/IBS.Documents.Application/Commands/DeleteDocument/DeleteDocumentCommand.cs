using IBS.BuildingBlocks.Application.Commands;

namespace IBS.Documents.Application.Commands.DeleteDocument;

/// <summary>
/// Command to archive (soft-delete) a document.
/// </summary>
public sealed record DeleteDocumentCommand(
    Guid TenantId,
    Guid DocumentId
) : ICommand;
