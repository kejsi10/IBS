namespace IBS.Documents.Application.Services;

/// <summary>
/// Data needed to generate a COI for a policy.
/// </summary>
public sealed record PolicyCOIData(
    string PolicyNumber,
    string ClientName,
    string CarrierName,
    DateTimeOffset EffectiveDate,
    DateTimeOffset ExpirationDate,
    string LineOfBusiness,
    string[] CoverageSummary);

/// <summary>
/// Cross-context service interface for fetching policy data needed for COI generation.
/// </summary>
public interface IPolicyDataService
{
    /// <summary>
    /// Gets the COI data for a policy.
    /// </summary>
    /// <param name="policyId">The policy identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The COI data if the policy is found, otherwise null.</returns>
    Task<PolicyCOIData?> GetPolicyCOIDataAsync(Guid policyId, CancellationToken cancellationToken = default);
}
