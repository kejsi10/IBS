using IBS.PolicyAssistant.Domain.Aggregates.ReferenceDocument;
using IBS.PolicyAssistant.Domain.Enums;
using IBS.PolicyAssistant.Domain.Queries;
using Microsoft.EntityFrameworkCore;

namespace IBS.PolicyAssistant.Infrastructure.Persistence;

/// <summary>
/// Query implementation for reading <see cref="ReferenceDocument"/> data (no-tracking reads).
/// </summary>
public sealed class ReferenceDocumentQueries(DbContext context) : IReferenceDocumentQueries
{
    /// <inheritdoc />
    public async Task<IReadOnlyList<ReferenceDocument>> GetAllAsync(DocumentCategory? category = null, CancellationToken cancellationToken = default)
    {
        IQueryable<ReferenceDocument> query = context.Set<ReferenceDocument>()
            .AsNoTracking()
            .Include(d => d.Chunks);

        if (category.HasValue)
            query = query.Where(d => d.Category == category.Value);

        return await query.OrderBy(d => d.Title).ToListAsync(cancellationToken);
    }
}
