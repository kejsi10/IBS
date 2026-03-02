using IBS.BuildingBlocks.Infrastructure.Persistence;
using IBS.PolicyAssistant.Domain.Aggregates.ReferenceDocument;
using IBS.PolicyAssistant.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace IBS.PolicyAssistant.Infrastructure.Persistence;

/// <summary>
/// Repository implementation for the <see cref="ReferenceDocument"/> aggregate.
/// </summary>
public sealed class ReferenceDocumentRepository(DbContext context)
    : Repository<ReferenceDocument, DbContext>(context), IReferenceDocumentRepository
{
    /// <inheritdoc />
    public override async Task<ReferenceDocument?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(d => d.Chunks)
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
    }
}
