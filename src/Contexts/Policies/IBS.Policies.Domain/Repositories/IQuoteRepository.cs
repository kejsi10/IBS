using IBS.Policies.Domain.Aggregates.Quote;

namespace IBS.Policies.Domain.Repositories;

/// <summary>
/// Repository interface for Quote aggregate operations.
/// </summary>
public interface IQuoteRepository
{
    /// <summary>
    /// Gets a quote by its identifier with all carriers loaded.
    /// </summary>
    /// <param name="id">The quote identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The quote if found; otherwise, null.</returns>
    Task<Quote?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new quote.
    /// </summary>
    /// <param name="quote">The quote to add.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task AddAsync(Quote quote, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing quote.
    /// </summary>
    /// <param name="quote">The quote to update.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task UpdateAsync(Quote quote, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets quotes that have expired but are not in a terminal state.
    /// </summary>
    /// <param name="asOfDate">The date to check for expiration.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of expired quotes.</returns>
    Task<IReadOnlyList<Quote>> GetExpiredQuotesAsync(DateOnly asOfDate, CancellationToken cancellationToken = default);
}
