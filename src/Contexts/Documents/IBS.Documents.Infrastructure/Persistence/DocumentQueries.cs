using IBS.Documents.Domain.Aggregates.Document;
using IBS.Documents.Domain.Queries;
using Microsoft.EntityFrameworkCore;

namespace IBS.Documents.Infrastructure.Persistence;

/// <summary>
/// Query implementation for documents.
/// </summary>
public sealed class DocumentQueries : IDocumentQueries
{
    private readonly DbContext _context;

    /// <summary>
    /// Initializes a new instance of the DocumentQueries class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public DocumentQueries(DbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<DocumentReadModel?> GetByIdAsync(Guid tenantId, Guid documentId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<Document>()
            .AsNoTracking()
            .Where(d => d.TenantId == tenantId && d.Id == documentId)
            .Select(d => new DocumentReadModel(
                d.Id,
                d.TenantId,
                d.EntityType,
                d.EntityId,
                d.FileName,
                d.ContentType,
                d.FileSizeBytes,
                d.BlobKey,
                d.Category,
                d.Version,
                d.IsArchived,
                d.UploadedBy,
                d.Description,
                d.UploadedAt))
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<DocumentSearchResult> SearchAsync(DocumentSearchFilter filter, CancellationToken cancellationToken = default)
    {
        var query = _context.Set<Document>()
            .AsNoTracking()
            .Where(d => d.TenantId == filter.TenantId);

        if (!filter.IncludeArchived)
            query = query.Where(d => !d.IsArchived);

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            query = query.Where(d => d.FileName.Contains(filter.SearchTerm) ||
                                     (d.Description != null && d.Description.Contains(filter.SearchTerm)));

        if (filter.Category.HasValue)
            query = query.Where(d => d.Category == filter.Category.Value);

        if (filter.EntityType.HasValue)
            query = query.Where(d => d.EntityType == filter.EntityType.Value);

        if (filter.EntityId.HasValue)
            query = query.Where(d => d.EntityId == filter.EntityId.Value);

        var totalCount = await query.CountAsync(cancellationToken);
        var totalPages = (int)Math.Ceiling(totalCount / (double)filter.PageSize);

        var items = await query
            .OrderByDescending(d => d.UploadedAt)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(d => new DocumentListItemReadModel(
                d.Id,
                d.EntityType,
                d.EntityId,
                d.FileName,
                d.ContentType,
                d.FileSizeBytes,
                d.Category,
                d.Version,
                d.IsArchived,
                d.UploadedBy,
                d.UploadedAt))
            .ToListAsync(cancellationToken);

        return new DocumentSearchResult(items, totalCount, filter.Page, filter.PageSize, totalPages);
    }
}
