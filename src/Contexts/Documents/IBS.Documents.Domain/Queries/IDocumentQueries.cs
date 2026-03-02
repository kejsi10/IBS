using IBS.Documents.Domain.Aggregates.Document;

namespace IBS.Documents.Domain.Queries;

/// <summary>
/// Read model for a document.
/// </summary>
public sealed record DocumentReadModel(
    Guid Id,
    Guid TenantId,
    DocumentEntityType EntityType,
    Guid? EntityId,
    string FileName,
    string ContentType,
    long FileSizeBytes,
    string BlobKey,
    DocumentCategory Category,
    int Version,
    bool IsArchived,
    string UploadedBy,
    string? Description,
    DateTimeOffset UploadedAt);

/// <summary>
/// Read model for a document list item.
/// </summary>
public sealed record DocumentListItemReadModel(
    Guid Id,
    DocumentEntityType EntityType,
    Guid? EntityId,
    string FileName,
    string ContentType,
    long FileSizeBytes,
    DocumentCategory Category,
    int Version,
    bool IsArchived,
    string UploadedBy,
    DateTimeOffset UploadedAt);

/// <summary>
/// Filter for searching documents.
/// </summary>
public sealed record DocumentSearchFilter(
    Guid TenantId,
    string? SearchTerm = null,
    DocumentCategory? Category = null,
    DocumentEntityType? EntityType = null,
    Guid? EntityId = null,
    bool IncludeArchived = false,
    int Page = 1,
    int PageSize = 20);

/// <summary>
/// Result of a document search.
/// </summary>
public sealed record DocumentSearchResult(
    IReadOnlyList<DocumentListItemReadModel> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages);

/// <summary>
/// Query interface for documents.
/// </summary>
public interface IDocumentQueries
{
    /// <summary>
    /// Gets a document by its identifier.
    /// </summary>
    /// <param name="tenantId">The tenant identifier.</param>
    /// <param name="documentId">The document identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The document read model if found, otherwise null.</returns>
    Task<DocumentReadModel?> GetByIdAsync(Guid tenantId, Guid documentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches documents with filters.
    /// </summary>
    /// <param name="filter">The search filter.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The paginated search result.</returns>
    Task<DocumentSearchResult> SearchAsync(DocumentSearchFilter filter, CancellationToken cancellationToken = default);
}
