using IBS.BuildingBlocks.Application.Queries;

namespace IBS.Policies.Application.Queries.GetRenewalComparison;

/// <summary>
/// Query to get a side-by-side renewal comparison for a policy.
/// </summary>
public sealed record GetRenewalComparisonQuery(
    Guid TenantId,
    Guid PolicyId
) : IQuery<RenewalComparisonDto?>;

/// <summary>
/// Top-level DTO for the renewal comparison view.
/// </summary>
public sealed record RenewalComparisonDto(
    CurrentPolicyInfo CurrentPolicy,
    IReadOnlyList<RenewalOfferDto> RenewalOffers
);

/// <summary>
/// Current policy summary for the comparison view.
/// </summary>
public sealed record CurrentPolicyInfo(
    string PolicyNumber,
    string? CarrierName,
    decimal TotalPremium,
    string Currency,
    DateOnly EffectiveDate,
    DateOnly ExpirationDate,
    IReadOnlyList<string> Coverages
);

/// <summary>
/// A single renewal carrier offer for comparison.
/// </summary>
public sealed record RenewalOfferDto(
    Guid QuoteId,
    Guid QuoteCarrierId,
    Guid CarrierId,
    string? CarrierName,
    decimal? PremiumAmount,
    string? Conditions,
    string? ProposedCoverages,
    string Status,
    decimal? PremiumDifference,
    decimal? PremiumDifferencePercent
);
