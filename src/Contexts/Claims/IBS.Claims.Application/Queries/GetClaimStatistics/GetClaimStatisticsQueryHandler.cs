using IBS.BuildingBlocks.Application;
using IBS.BuildingBlocks.Application.Queries;
using IBS.Claims.Application.DTOs;
using IBS.Claims.Domain.Queries;

namespace IBS.Claims.Application.Queries.GetClaimStatistics;

/// <summary>
/// Handler for the GetClaimStatisticsQuery.
/// </summary>
public sealed class GetClaimStatisticsQueryHandler(
    IClaimQueries claimQueries) : IQueryHandler<GetClaimStatisticsQuery, ClaimStatisticsDto>
{
    /// <inheritdoc />
    public async Task<Result<ClaimStatisticsDto>> Handle(GetClaimStatisticsQuery request, CancellationToken cancellationToken)
    {
        var stats = await claimQueries.GetStatisticsAsync(cancellationToken);

        return new ClaimStatisticsDto(
            stats.TotalClaims,
            stats.OpenClaims,
            stats.ClosedClaims,
            stats.DeniedClaims,
            stats.TotalClaimAmount,
            stats.TotalPaidAmount,
            stats.ClaimsByStatus,
            stats.ClaimsByLossType
        );
    }
}
