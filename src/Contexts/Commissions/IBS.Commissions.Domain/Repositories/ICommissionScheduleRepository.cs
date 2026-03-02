using IBS.Commissions.Domain.Aggregates.CommissionSchedule;

namespace IBS.Commissions.Domain.Repositories;

/// <summary>
/// Repository interface for the CommissionSchedule aggregate.
/// </summary>
public interface ICommissionScheduleRepository
{
    /// <summary>
    /// Gets a commission schedule by its identifier.
    /// </summary>
    /// <param name="id">The schedule identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The schedule if found, otherwise null.</returns>
    Task<CommissionSchedule?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new commission schedule.
    /// </summary>
    /// <param name="schedule">The schedule to add.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task AddAsync(CommissionSchedule schedule, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing commission schedule.
    /// </summary>
    /// <param name="schedule">The schedule to update.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task UpdateAsync(CommissionSchedule schedule, CancellationToken cancellationToken = default);
}
