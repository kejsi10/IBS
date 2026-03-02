using IBS.BuildingBlocks.Application;
using IBS.BuildingBlocks.Application.Queries;
using IBS.Carriers.Domain.ValueObjects;
using IBS.Policies.Application.Queries.GetPolicies;
using IBS.Policies.Domain.Repositories;
using IBS.BuildingBlocks.Domain.ValueObjects;
using IBS.Policies.Domain.ValueObjects;

namespace IBS.Policies.Application.Queries.GetExpiringPolicies;

/// <summary>
/// Handler for the GetExpiringPoliciesQuery.
/// </summary>
public sealed class GetExpiringPoliciesQueryHandler(
    IPolicyRepository policyRepository) : IQueryHandler<GetExpiringPoliciesQuery, PolicyListResult>
{
    /// <inheritdoc />
    public async Task<Result<PolicyListResult>> Handle(GetExpiringPoliciesQuery request, CancellationToken cancellationToken)
    {
        var policies = await policyRepository.GetExpiringPoliciesAsync(
            request.StartDate,
            request.EndDate,
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
