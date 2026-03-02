using IBS.BuildingBlocks.Application;
using IBS.BuildingBlocks.Application.Queries;
using IBS.Carriers.Domain.ValueObjects;
using IBS.Policies.Domain.Repositories;
using IBS.BuildingBlocks.Domain.ValueObjects;
using IBS.Policies.Domain.ValueObjects;

namespace IBS.Policies.Application.Queries.GetPolicies;

/// <summary>
/// Handler for the GetPoliciesQuery.
/// </summary>
public sealed class GetPoliciesQueryHandler(
    IPolicyRepository policyRepository) : IQueryHandler<GetPoliciesQuery, PolicyListResult>
{
    /// <inheritdoc />
    public async Task<Result<PolicyListResult>> Handle(GetPoliciesQuery request, CancellationToken cancellationToken)
    {
        var filter = new PolicySearchFilter
        {
            SearchTerm = request.SearchTerm,
            ClientId = request.ClientId,
            CarrierId = request.CarrierId,
            Status = request.Status,
            LineOfBusiness = request.LineOfBusiness,
            EffectiveDateFrom = request.EffectiveDateFrom,
            EffectiveDateTo = request.EffectiveDateTo,
            ExpirationDateFrom = request.ExpirationDateFrom,
            ExpirationDateTo = request.ExpirationDateTo,
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
            null, // ClientName would come from a join or separate query
            p.CarrierId,
            null, // CarrierName would come from a join or separate query
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
