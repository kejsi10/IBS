using IBS.BuildingBlocks.Domain;

namespace IBS.PolicyAssistant.Domain.Aggregates.ReferenceDocument;

/// <summary>
/// Represents a text chunk of a reference document for full-text search.
/// Child entity of the <see cref="ReferenceDocument"/> aggregate.
/// </summary>
public sealed class DocumentChunk : Entity
{
    /// <summary>
    /// Gets the reference document this chunk belongs to.
    /// </summary>
    public Guid ReferenceDocumentId { get; private set; }

    /// <summary>
    /// Gets the zero-based index of this chunk within the document.
    /// </summary>
    public int ChunkIndex { get; private set; }

    /// <summary>
    /// Gets the text content of this chunk.
    /// </summary>
    public string Content { get; private set; } = string.Empty;

    /// <summary>
    /// Required by EF Core.
    /// </summary>
    private DocumentChunk() { }

    /// <summary>
    /// Creates a new document chunk.
    /// </summary>
    /// <param name="referenceDocumentId">The parent document identifier.</param>
    /// <param name="chunkIndex">The zero-based chunk index.</param>
    /// <param name="content">The chunk text content.</param>
    /// <returns>A new <see cref="DocumentChunk"/>.</returns>
    public static DocumentChunk Create(Guid referenceDocumentId, int chunkIndex, string content)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(content);

        return new DocumentChunk
        {
            ReferenceDocumentId = referenceDocumentId,
            ChunkIndex = chunkIndex,
            Content = content
        };
    }
}
