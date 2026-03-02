using IBS.Carriers.Domain.Aggregates.Carrier;
using IBS.Carriers.Domain.ValueObjects;

namespace IBS.Carriers.Domain.Queries;

/// <summary>
/// Query interface for reading Carrier data with no-tracking.
/// </summary>
public interface ICarrierQueries
{
    /// <summary>
    /// Gets a carrier by its identifier.
    /// </summary>
    /// <param name="id">The carrier identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The carrier if found; otherwise, null.</returns>
    Task<Carrier?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a carrier by its code.
    /// </summary>
    /// <param name="code">The carrier code.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The carrier if found; otherwise, null.</returns>
    Task<Carrier?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all carriers.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>All carriers.</returns>
    Task<IReadOnlyList<Carrier>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets carriers by status.
    /// </summary>
    /// <param name="status">The carrier status.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Carriers with the specified status.</returns>
    Task<IReadOnlyList<Carrier>> GetByStatusAsync(CarrierStatus status, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets carriers that offer a specific line of business.
    /// </summary>
    /// <param name="lineOfBusiness">The line of business.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Carriers offering the specified line of business.</returns>
    Task<IReadOnlyList<Carrier>> GetByLineOfBusinessAsync(LineOfBusiness lineOfBusiness, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets carriers that cover a specific state for a line of business.
    /// </summary>
    /// <param name="lineOfBusiness">The line of business.</param>
    /// <param name="state">The state code (e.g., "CA", "TX").</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Carriers covering the specified state.</returns>
    Task<IReadOnlyList<Carrier>> GetByStateAndLineOfBusinessAsync(
        LineOfBusiness lineOfBusiness,
        string state,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a carrier with the given code exists.
    /// </summary>
    /// <param name="code">The carrier code.</param>
    /// <param name="excludeId">Optional carrier ID to exclude from the check.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if a carrier with the code exists; otherwise, false.</returns>
    Task<bool> ExistsByCodeAsync(string code, Guid? excludeId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches carriers by name.
    /// </summary>
    /// <param name="searchTerm">The search term.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Carriers matching the search term.</returns>
    Task<IReadOnlyList<Carrier>> SearchByNameAsync(string searchTerm, CancellationToken cancellationToken = default);
}
