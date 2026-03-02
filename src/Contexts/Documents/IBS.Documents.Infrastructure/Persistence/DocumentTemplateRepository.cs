using IBS.Documents.Domain.Aggregates.DocumentTemplate;
using IBS.Documents.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace IBS.Documents.Infrastructure.Persistence;

/// <summary>
/// Repository implementation for the DocumentTemplate aggregate.
/// </summary>
public sealed class DocumentTemplateRepository : IDocumentTemplateRepository
{
    private readonly DbContext _context;
    private readonly DbSet<DocumentTemplate> _templates;

    /// <summary>
    /// Initializes a new instance of the DocumentTemplateRepository class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public DocumentTemplateRepository(DbContext context)
    {
        _context = context;
        _templates = context.Set<DocumentTemplate>();
    }

    /// <inheritdoc />
    public async Task<DocumentTemplate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _templates.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<DocumentTemplate>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _templates
            .OrderBy(t => t.Name)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<DocumentTemplate>> GetActiveByTypeAsync(TemplateType templateType, CancellationToken cancellationToken = default)
    {
        return await _templates
            .Where(t => t.TemplateType == templateType && t.IsActive)
            .OrderBy(t => t.Name)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddAsync(DocumentTemplate template, CancellationToken cancellationToken = default)
    {
        await _templates.AddAsync(template, cancellationToken);
    }

    /// <inheritdoc />
    public Task UpdateAsync(DocumentTemplate template, CancellationToken cancellationToken = default)
    {
        // Entity is already tracked by EF change tracker via GetByIdAsync.
        return Task.CompletedTask;
    }
}
