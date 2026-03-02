using IBS.Commissions.Domain.Aggregates.CommissionStatement;

namespace IBS.Commissions.Domain.Repositories;

/// <summary>
/// Repository interface for the CommissionStatement aggregate.
/// </summary>
public interface ICommissionStatementRepository
{
    /// <summary>
    /// Gets a commission statement by its identifier, including line items and producer splits.
    /// </summary>
    /// <param name="id">The statement identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The statement if found, otherwise null.</returns>
    Task<CommissionStatement?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new commission statement.
    /// </summary>
    /// <param name="statement">The statement to add.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task AddAsync(CommissionStatement statement, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing commission statement.
    /// </summary>
    /// <param name="statement">The statement to update.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task UpdateAsync(CommissionStatement statement, CancellationToken cancellationToken = default);
}
