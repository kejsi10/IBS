using IBS.BuildingBlocks.Application.Commands;
using IBS.Documents.Domain.Aggregates.Document;

namespace IBS.Documents.Application.Commands.UploadDocument;

/// <summary>
/// Command to upload a new document.
/// </summary>
public sealed record UploadDocumentCommand(
    Guid TenantId,
    string UserId,
    DocumentEntityType EntityType,
    Guid? EntityId,
    string FileName,
    string ContentType,
    long FileSizeBytes,
    Stream FileContent,
    DocumentCategory Category,
    string? Description = null
) : ICommand<Guid>;
