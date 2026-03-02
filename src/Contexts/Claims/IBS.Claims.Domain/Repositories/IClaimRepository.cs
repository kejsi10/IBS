using IBS.Claims.Domain.Aggregates.Claim;
using IBS.BuildingBlocks.Domain.ValueObjects;
using IBS.Claims.Domain.ValueObjects;

namespace IBS.Claims.Domain.Repositories;

/// <summary>
/// Repository interface for the Claim aggregate.
/// </summary>
public interface IClaimRepository
{
    /// <summary>
    /// Gets a claim by its identifier.
    /// </summary>
    /// <param name="id">The claim identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The claim if found, otherwise null.</returns>
    Task<Claim?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets claims by policy identifier.
    /// </summary>
    /// <param name="policyId">The policy identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A list of claims for the policy.</returns>
    Task<IReadOnlyList<Claim>> GetByPolicyIdAsync(Guid policyId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets claims by client identifier.
    /// </summary>
    /// <param name="clientId">The client identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A list of claims for the client.</returns>
    Task<IReadOnlyList<Claim>> GetByClientIdAsync(Guid clientId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new claim.
    /// </summary>
    /// <param name="claim">The claim to add.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task AddAsync(Claim claim, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing claim.
    /// </summary>
    /// <param name="claim">The claim to update.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task UpdateAsync(Claim claim, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the count of claims by status.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A dictionary of claim status to count.</returns>
    Task<Dictionary<ClaimStatus, int>> GetClaimCountByStatusAsync(CancellationToken cancellationToken = default);
}
