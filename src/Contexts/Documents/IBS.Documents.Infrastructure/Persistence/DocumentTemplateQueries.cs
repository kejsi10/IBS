using IBS.Documents.Domain.Aggregates.DocumentTemplate;
using IBS.Documents.Domain.Queries;
using Microsoft.EntityFrameworkCore;

namespace IBS.Documents.Infrastructure.Persistence;

/// <summary>
/// Query implementation for document templates.
/// </summary>
public sealed class DocumentTemplateQueries : IDocumentTemplateQueries
{
    private readonly DbContext _context;

    /// <summary>
    /// Initializes a new instance of the DocumentTemplateQueries class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public DocumentTemplateQueries(DbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<DocumentTemplateReadModel?> GetByIdAsync(Guid tenantId, Guid templateId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<DocumentTemplate>()
            .AsNoTracking()
            .Where(t => t.TenantId == tenantId && t.Id == templateId)
            .Select(t => new DocumentTemplateReadModel(
                t.Id,
                t.TenantId,
                t.Name,
                t.Description,
                t.TemplateType,
                t.Content,
                t.IsActive,
                t.Version,
                t.CreatedBy))
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<DocumentTemplateSearchResult> SearchAsync(DocumentTemplateSearchFilter filter, CancellationToken cancellationToken = default)
    {
        var query = _context.Set<DocumentTemplate>()
            .AsNoTracking()
            .Where(t => t.TenantId == filter.TenantId);

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            query = query.Where(t => t.Name.Contains(filter.SearchTerm) || t.Description.Contains(filter.SearchTerm));

        if (filter.TemplateType.HasValue)
            query = query.Where(t => t.TemplateType == filter.TemplateType.Value);

        if (filter.IsActive.HasValue)
            query = query.Where(t => t.IsActive == filter.IsActive.Value);

        var totalCount = await query.CountAsync(cancellationToken);
        var totalPages = (int)Math.Ceiling(totalCount / (double)filter.PageSize);

        var items = await query
            .OrderBy(t => t.Name)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(t => new DocumentTemplateListItemReadModel(
                t.Id,
                t.Name,
                t.Description,
                t.TemplateType,
                t.IsActive,
                t.Version,
                t.CreatedBy))
            .ToListAsync(cancellationToken);

        return new DocumentTemplateSearchResult(items, totalCount, filter.Page, filter.PageSize, totalPages);
    }
}
