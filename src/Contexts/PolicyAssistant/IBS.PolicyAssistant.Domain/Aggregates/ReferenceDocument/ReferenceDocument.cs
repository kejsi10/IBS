using IBS.BuildingBlocks.Domain;
using IBS.PolicyAssistant.Domain.Enums;

namespace IBS.PolicyAssistant.Domain.Aggregates.ReferenceDocument;

/// <summary>
/// Aggregate root representing a reference document used for RAG-based policy validation.
/// Can be system-wide (TenantId = null) or tenant-specific.
/// </summary>
public sealed class ReferenceDocument : AggregateRoot
{
    private readonly List<DocumentChunk> _chunks = [];

    /// <summary>
    /// Gets the optional tenant identifier. Null means the document is system-wide.
    /// </summary>
    public Guid? TenantId { get; private set; }

    /// <summary>
    /// Gets the title of the reference document.
    /// </summary>
    public string Title { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the category of the document (Regulation, SamplePolicy, ValidationRule).
    /// </summary>
    public DocumentCategory Category { get; private set; }

    /// <summary>
    /// Gets the line of business this document applies to (optional).
    /// </summary>
    public string? LineOfBusiness { get; private set; }

    /// <summary>
    /// Gets the state this document applies to (optional, e.g., "CA", "TX").
    /// </summary>
    public string? State { get; private set; }

    /// <summary>
    /// Gets the full text content of the document.
    /// </summary>
    public string Content { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the source or origin of the document (e.g., URL, file name, regulator name).
    /// </summary>
    public string? Source { get; private set; }

    /// <summary>
    /// Gets the text chunks of the document for full-text search.
    /// </summary>
    public IReadOnlyCollection<DocumentChunk> Chunks => _chunks.AsReadOnly();

    /// <summary>
    /// Required by EF Core.
    /// </summary>
    private ReferenceDocument() { }

    /// <summary>
    /// Creates a new reference document.
    /// </summary>
    /// <param name="title">The document title.</param>
    /// <param name="category">The document category.</param>
    /// <param name="content">The full text content.</param>
    /// <param name="tenantId">Optional tenant identifier (null for system-wide).</param>
    /// <param name="lineOfBusiness">Optional line of business.</param>
    /// <param name="state">Optional state code.</param>
    /// <param name="source">Optional source description.</param>
    /// <returns>A new <see cref="ReferenceDocument"/>.</returns>
    public static ReferenceDocument Create(
        string title,
        DocumentCategory category,
        string content,
        Guid? tenantId = null,
        string? lineOfBusiness = null,
        string? state = null,
        string? source = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(content);

        return new ReferenceDocument
        {
            TenantId = tenantId,
            Title = title.Trim(),
            Category = category,
            Content = content,
            LineOfBusiness = lineOfBusiness,
            State = state,
            Source = source
        };
    }

    /// <summary>
    /// Chunks the document content and stores the chunks for full-text search.
    /// Uses a sliding window of approximately 1000 characters with 200-character overlap.
    /// </summary>
    /// <param name="chunkSize">The target size of each chunk in characters.</param>
    /// <param name="overlap">The number of characters to overlap between chunks.</param>
    public void CreateChunks(int chunkSize = 1000, int overlap = 200)
    {
        _chunks.Clear();

        if (string.IsNullOrWhiteSpace(Content))
            return;

        var text = Content;
        var index = 0;
        var chunkIndex = 0;

        while (index < text.Length)
        {
            var end = Math.Min(index + chunkSize, text.Length);

            // Try to break at a sentence boundary
            if (end < text.Length)
            {
                var lastPeriod = text.LastIndexOf('.', end, Math.Min(end - index, 100));
                if (lastPeriod > index)
                    end = lastPeriod + 1;
            }

            var chunkContent = text[index..end].Trim();
            if (!string.IsNullOrWhiteSpace(chunkContent))
            {
                _chunks.Add(DocumentChunk.Create(Id, chunkIndex++, chunkContent));
            }

            index = Math.Max(index + 1, end - overlap);
        }

        MarkAsUpdated();
    }
}
