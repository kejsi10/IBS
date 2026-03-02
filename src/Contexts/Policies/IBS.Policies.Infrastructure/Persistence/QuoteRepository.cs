using IBS.Policies.Domain.Aggregates.Quote;
using IBS.Policies.Domain.Repositories;
using IBS.BuildingBlocks.Domain.ValueObjects;
using IBS.Policies.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace IBS.Policies.Infrastructure.Persistence;

/// <summary>
/// Repository implementation for Quote aggregate.
/// </summary>
public sealed class QuoteRepository : IQuoteRepository
{
    private readonly DbContext _context;
    private readonly DbSet<Quote> _quotes;

    /// <summary>
    /// Initializes a new instance of the QuoteRepository class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public QuoteRepository(DbContext context)
    {
        _context = context;
        _quotes = context.Set<Quote>();
    }

    /// <inheritdoc />
    public async Task<Quote?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _quotes
            .Include(q => q.Carriers)
            .FirstOrDefaultAsync(q => q.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddAsync(Quote quote, CancellationToken cancellationToken = default)
    {
        await _quotes.AddAsync(quote, cancellationToken);
    }

    /// <inheritdoc />
    public Task UpdateAsync(Quote quote, CancellationToken cancellationToken = default)
    {
        // Entity is already tracked by EF change tracker via GetByIdAsync.
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Quote>> GetExpiredQuotesAsync(DateOnly asOfDate, CancellationToken cancellationToken = default)
    {
        return await _quotes
            .Include(q => q.Carriers)
            .Where(q => q.ExpiresAt < asOfDate)
            .Where(q => q.Status != QuoteStatus.Accepted
                     && q.Status != QuoteStatus.Expired
                     && q.Status != QuoteStatus.Cancelled)
            .ToListAsync(cancellationToken);
    }
}
