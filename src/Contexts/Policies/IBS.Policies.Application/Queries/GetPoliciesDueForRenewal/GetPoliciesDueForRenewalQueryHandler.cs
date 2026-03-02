using IBS.BuildingBlocks.Application;
using IBS.BuildingBlocks.Application.Queries;
using IBS.Carriers.Domain.ValueObjects;
using IBS.Policies.Application.Queries.GetPolicies;
using IBS.Policies.Domain.Repositories;
using IBS.BuildingBlocks.Domain.ValueObjects;
using IBS.Policies.Domain.ValueObjects;

namespace IBS.Policies.Application.Queries.GetPoliciesDueForRenewal;

/// <summary>
/// Handler for the GetPoliciesDueForRenewalQuery.
/// </summary>
public sealed class GetPoliciesDueForRenewalQueryHandler(
    IPolicyRepository policyRepository) : IQueryHandler<GetPoliciesDueForRenewalQuery, PolicyListResult>
{
    /// <inheritdoc />
    public async Task<Result<PolicyListResult>> Handle(GetPoliciesDueForRenewalQuery request, CancellationToken cancellationToken)
    {
        var policies = await policyRepository.GetPoliciesDueForRenewalAsync(
            request.DaysUntilExpiration,
            cancellationToken);

        var totalCount = policies.Count;
        var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);

        var pagedPolicies = policies
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        var items = pagedPolicies.Select(p => new PolicyListItemDto(
            p.Id,
            p.PolicyNumber.Value,
            p.ClientId,
            null,
            p.CarrierId,
            null,
            p.LineOfBusiness.GetDisplayName(),
            p.PolicyType,
            p.Status.GetDisplayName(),
            p.EffectivePeriod.EffectiveDate,
            p.EffectivePeriod.ExpirationDate,
            p.TotalPremium.Amount,
            p.TotalPremium.Currency,
            p.CreatedAt
        )).ToList();

        return new PolicyListResult(
            items,
            totalCount,
            request.PageNumber,
            request.PageSize,
            totalPages
        );
    }
}
