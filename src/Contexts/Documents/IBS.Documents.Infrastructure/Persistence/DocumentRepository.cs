using IBS.Documents.Domain.Aggregates.Document;
using IBS.Documents.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace IBS.Documents.Infrastructure.Persistence;

/// <summary>
/// Repository implementation for the Document aggregate.
/// </summary>
public sealed class DocumentRepository : IDocumentRepository
{
    private readonly DbContext _context;
    private readonly DbSet<Document> _documents;

    /// <summary>
    /// Initializes a new instance of the DocumentRepository class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public DocumentRepository(DbContext context)
    {
        _context = context;
        _documents = context.Set<Document>();
    }

    /// <inheritdoc />
    public async Task<Document?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _documents.FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Document>> GetByEntityAsync(DocumentEntityType entityType, Guid entityId, CancellationToken cancellationToken = default)
    {
        return await _documents
            .Where(d => d.EntityType == entityType && d.EntityId == entityId && !d.IsArchived)
            .OrderByDescending(d => d.UploadedAt)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddAsync(Document document, CancellationToken cancellationToken = default)
    {
        await _documents.AddAsync(document, cancellationToken);
    }

    /// <inheritdoc />
    public Task UpdateAsync(Document document, CancellationToken cancellationToken = default)
    {
        // Entity is already tracked by EF change tracker via GetByIdAsync.
        return Task.CompletedTask;
    }
}
