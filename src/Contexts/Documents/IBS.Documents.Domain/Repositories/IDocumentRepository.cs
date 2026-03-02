using IBS.Documents.Domain.Aggregates.Document;

namespace IBS.Documents.Domain.Repositories;

/// <summary>
/// Repository interface for the Document aggregate.
/// </summary>
public interface IDocumentRepository
{
    /// <summary>
    /// Gets a document by its identifier.
    /// </summary>
    /// <param name="id">The document identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The document if found, otherwise null.</returns>
    Task<Document?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets documents linked to a specific entity.
    /// </summary>
    /// <param name="entityType">The entity type.</param>
    /// <param name="entityId">The entity identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A list of documents linked to the entity.</returns>
    Task<IReadOnlyList<Document>> GetByEntityAsync(DocumentEntityType entityType, Guid entityId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new document.
    /// </summary>
    /// <param name="document">The document to add.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task AddAsync(Document document, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing document.
    /// </summary>
    /// <param name="document">The document to update.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task UpdateAsync(Document document, CancellationToken cancellationToken = default);
}
