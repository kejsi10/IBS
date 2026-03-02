using IBS.BuildingBlocks.Application;
using IBS.BuildingBlocks.Application.Queries;
using IBS.Claims.Application.DTOs;
using IBS.Claims.Domain.Queries;

namespace IBS.Claims.Application.Queries.GetClaims;

/// <summary>
/// Handler for the GetClaimsQuery.
/// </summary>
public sealed class GetClaimsQueryHandler(
    IClaimQueries claimQueries) : IQueryHandler<GetClaimsQuery, ClaimListResult>
{
    /// <inheritdoc />
    public async Task<Result<ClaimListResult>> Handle(GetClaimsQuery request, CancellationToken cancellationToken)
    {
        var filter = new ClaimSearchFilter
        {
            SearchTerm = request.SearchTerm,
            Status = request.Status,
            PolicyId = request.PolicyId,
            ClientId = request.ClientId,
            LossType = request.LossType,
            LossDateFrom = request.LossDateFrom,
            LossDateTo = request.LossDateTo,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            SortBy = request.SortBy,
            SortDirection = request.SortDirection
        };

        var result = await claimQueries.SearchAsync(filter, cancellationToken);

        var items = result.Claims.Select(c => new ClaimListItemDto(
            c.Id,
            c.ClaimNumber,
            c.PolicyId,
            c.ClientId,
            c.Status,
            c.LossDate,
            c.ReportedDate,
            c.LossType,
            c.LossAmount,
            c.LossAmountCurrency,
            c.ClaimAmount,
            c.ClaimAmountCurrency,
            c.AssignedAdjusterId,
            c.CreatedAt
        )).ToList();

        return new ClaimListResult(
            items,
            result.TotalCount,
            result.PageNumber,
            result.PageSize,
            result.TotalPages
        );
    }
}
