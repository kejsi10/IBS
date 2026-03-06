using IBS.BuildingBlocks.Application;
using IBS.BuildingBlocks.Application.Queries;
using IBS.Policies.Domain.Queries;
using IBS.Policies.Domain.Repositories;

namespace IBS.Policies.Application.Queries.GetRenewalComparison;

/// <summary>
/// Handler for GetRenewalComparisonQuery.
/// </summary>
public sealed class GetRenewalComparisonQueryHandler(
    IPolicyRepository policyRepository,
    IQuoteQueries quoteQueries) : IQueryHandler<GetRenewalComparisonQuery, RenewalComparisonDto?>
{
    /// <inheritdoc />
    public async Task<Result<RenewalComparisonDto?>> Handle(GetRenewalComparisonQuery request, CancellationToken cancellationToken)
    {
        var policy = await policyRepository.GetByIdAsync(request.PolicyId, cancellationToken);
        if (policy is null)
            return (RenewalComparisonDto?)null;

        var currentPolicy = new CurrentPolicyInfo(
            policy.PolicyNumber.Value,
            null, // CarrierName resolved at API presentation layer
            policy.TotalPremium.Amount,
            policy.TotalPremium.Currency,
            policy.EffectivePeriod.EffectiveDate,
            policy.EffectivePeriod.ExpirationDate,
            policy.Coverages
                .Where(c => c.IsActive)
                .Select(c => c.Name)
                .ToList());

        var renewalQuotes = await quoteQueries.GetRenewalQuotesAsync(
            request.TenantId,
            request.PolicyId,
            cancellationToken);

        var offers = renewalQuotes
            .SelectMany(q => q.Carriers.Select(c => new RenewalOfferDto(
                q.Id,
                c.Id,
                c.CarrierId,
                c.CarrierName,
                c.PremiumAmount,
                c.Conditions,
                c.ProposedCoverages,
                c.Status,
                c.PremiumAmount.HasValue
                    ? c.PremiumAmount.Value - policy.TotalPremium.Amount
                    : null,
                c.PremiumAmount.HasValue && policy.TotalPremium.Amount != 0
                    ? Math.Round((c.PremiumAmount.Value - policy.TotalPremium.Amount) / policy.TotalPremium.Amount * 100, 2)
                    : null)))
            .ToList();

        return new RenewalComparisonDto(currentPolicy, offers);
    }
}
