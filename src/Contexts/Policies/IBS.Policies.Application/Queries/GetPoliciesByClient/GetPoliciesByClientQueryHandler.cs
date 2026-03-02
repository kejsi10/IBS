using IBS.BuildingBlocks.Application;
using IBS.BuildingBlocks.Application.Queries;
using IBS.Carriers.Domain.ValueObjects;
using IBS.Policies.Application.Queries.GetPolicies;
using IBS.Policies.Domain.Repositories;
using IBS.BuildingBlocks.Domain.ValueObjects;
using IBS.Policies.Domain.ValueObjects;

namespace IBS.Policies.Application.Queries.GetPoliciesByClient;

/// <summary>
/// Handler for the GetPoliciesByClientQuery.
/// </summary>
public sealed class GetPoliciesByClientQueryHandler(
    IPolicyRepository policyRepository) : IQueryHandler<GetPoliciesByClientQuery, PolicyListResult>
{
    /// <inheritdoc />
    public async Task<Result<PolicyListResult>> Handle(GetPoliciesByClientQuery request, CancellationToken cancellationToken)
    {
        var filter = new PolicySearchFilter
        {
            ClientId = request.ClientId,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            SortBy = request.SortBy,
            SortDirection = request.SortDirection
        };

        var searchResult = await policyRepository.SearchAsync(filter, cancellationToken);

        var items = searchResult.Policies.Select(p => new PolicyListItemDto(
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
            searchResult.TotalCount,
            searchResult.PageNumber,
            searchResult.PageSize,
            searchResult.TotalPages
        );
    }
}
