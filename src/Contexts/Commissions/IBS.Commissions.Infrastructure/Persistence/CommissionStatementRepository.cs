using IBS.Commissions.Domain.Aggregates.CommissionStatement;
using IBS.Commissions.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace IBS.Commissions.Infrastructure.Persistence;

/// <summary>
/// Repository implementation for the CommissionStatement aggregate.
/// </summary>
public sealed class CommissionStatementRepository : ICommissionStatementRepository
{
    private readonly DbContext _context;
    private readonly DbSet<CommissionStatement> _statements;

    /// <summary>
    /// Initializes a new instance of the CommissionStatementRepository class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public CommissionStatementRepository(DbContext context)
    {
        _context = context;
        _statements = context.Set<CommissionStatement>();
    }

    /// <inheritdoc />
    public async Task<CommissionStatement?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _statements
            .Include(s => s.LineItems)
            .Include(s => s.ProducerSplits)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddAsync(CommissionStatement statement, CancellationToken cancellationToken = default)
    {
        await _statements.AddAsync(statement, cancellationToken);
    }

    /// <inheritdoc />
    public Task UpdateAsync(CommissionStatement statement, CancellationToken cancellationToken = default)
    {
        // Entity is already tracked by EF change tracker via GetByIdAsync.
        return Task.CompletedTask;
    }
}
