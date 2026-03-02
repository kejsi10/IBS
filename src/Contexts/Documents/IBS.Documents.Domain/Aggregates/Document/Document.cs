using IBS.BuildingBlocks.Domain;
using IBS.Documents.Domain.Events;

namespace IBS.Documents.Domain.Aggregates.Document;

/// <summary>
/// Represents a document stored in the system. This is the aggregate root for the Document aggregate.
/// </summary>
public sealed class Document : TenantAggregateRoot
{
    /// <summary>
    /// Gets the type of entity this document is linked to.
    /// </summary>
    public DocumentEntityType EntityType { get; private set; }

    /// <summary>
    /// Gets the identifier of the linked entity, if any.
    /// </summary>
    public Guid? EntityId { get; private set; }

    /// <summary>
    /// Gets the original file name.
    /// </summary>
    public string FileName { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the MIME content type.
    /// </summary>
    public string ContentType { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the file size in bytes.
    /// </summary>
    public long FileSizeBytes { get; private set; }

    /// <summary>
    /// Gets the blob storage key used to retrieve the file.
    /// </summary>
    public string BlobKey { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the document category.
    /// </summary>
    public DocumentCategory Category { get; private set; }

    /// <summary>
    /// Gets the version number (starts at 1).
    /// </summary>
    public int Version { get; private set; } = 1;

    /// <summary>
    /// Gets a value indicating whether the document is archived.
    /// </summary>
    public bool IsArchived { get; private set; }

    /// <summary>
    /// Gets the identifier of the user who uploaded the document.
    /// </summary>
    public string UploadedBy { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the optional description.
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// Gets the date/time when the document was uploaded.
    /// </summary>
    public DateTimeOffset UploadedAt { get; private set; }

    private Document() { }

    /// <summary>
    /// Creates a new document.
    /// </summary>
    /// <param name="tenantId">The tenant identifier.</param>
    /// <param name="entityType">The type of linked entity.</param>
    /// <param name="entityId">The linked entity identifier (optional).</param>
    /// <param name="fileName">The original file name.</param>
    /// <param name="contentType">The MIME content type.</param>
    /// <param name="fileSizeBytes">The file size in bytes.</param>
    /// <param name="blobKey">The blob storage key.</param>
    /// <param name="category">The document category.</param>
    /// <param name="uploadedBy">The user who uploaded the document.</param>
    /// <param name="description">An optional description.</param>
    /// <returns>The new document.</returns>
    public static Document Create(
        Guid tenantId,
        DocumentEntityType entityType,
        Guid? entityId,
        string fileName,
        string contentType,
        long fileSizeBytes,
        string blobKey,
        DocumentCategory category,
        string uploadedBy,
        string? description = null)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException("File name cannot be empty.", nameof(fileName));
        if (string.IsNullOrWhiteSpace(contentType))
            throw new ArgumentException("Content type cannot be empty.", nameof(contentType));
        if (fileSizeBytes <= 0)
            throw new ArgumentException("File size must be greater than zero.", nameof(fileSizeBytes));
        if (string.IsNullOrWhiteSpace(blobKey))
            throw new ArgumentException("Blob key cannot be empty.", nameof(blobKey));
        if (string.IsNullOrWhiteSpace(uploadedBy))
            throw new ArgumentException("Uploaded by cannot be empty.", nameof(uploadedBy));

        var document = new Document
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            EntityType = entityType,
            EntityId = entityId,
            FileName = fileName,
            ContentType = contentType,
            FileSizeBytes = fileSizeBytes,
            BlobKey = blobKey,
            Category = category,
            Version = 1,
            IsArchived = false,
            UploadedBy = uploadedBy,
            Description = description,
            UploadedAt = DateTimeOffset.UtcNow
        };

        document.RaiseDomainEvent(new DocumentUploadedEvent(
            document.Id,
            tenantId,
            fileName,
            category,
            entityType,
            entityId));

        return document;
    }

    /// <summary>
    /// Archives the document.
    /// </summary>
    public void Archive()
    {
        if (IsArchived)
            throw new InvalidOperationException("Document is already archived.");

        IsArchived = true;
        RaiseDomainEvent(new DocumentArchivedEvent(Id, TenantId, FileName));
    }
}
