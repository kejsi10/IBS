using IBS.BuildingBlocks.Domain;
using IBS.Documents.Domain.Aggregates.Document;
using IBS.Documents.Domain.Aggregates.DocumentTemplate;

namespace IBS.Documents.Domain.Events;

/// <summary>
/// Event raised when a document is uploaded.
/// </summary>
public sealed record DocumentUploadedEvent(
    Guid DocumentId,
    Guid TenantId,
    string FileName,
    DocumentCategory Category,
    DocumentEntityType EntityType,
    Guid? EntityId
) : DomainEvent;

/// <summary>
/// Event raised when a document is archived.
/// </summary>
public sealed record DocumentArchivedEvent(
    Guid DocumentId,
    Guid TenantId,
    string FileName
) : DomainEvent;

/// <summary>
/// Event raised when a document template is created.
/// </summary>
public sealed record DocumentTemplateCreatedEvent(
    Guid TemplateId,
    Guid TenantId,
    string Name,
    TemplateType TemplateType
) : DomainEvent;

/// <summary>
/// Event raised when a document template is updated.
/// </summary>
public sealed record DocumentTemplateUpdatedEvent(
    Guid TemplateId,
    Guid TenantId,
    string Name,
    int NewVersion
) : DomainEvent;
