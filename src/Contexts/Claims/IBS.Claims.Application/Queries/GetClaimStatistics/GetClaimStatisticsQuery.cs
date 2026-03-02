using IBS.BuildingBlocks.Application.Queries;
using IBS.Claims.Application.DTOs;

namespace IBS.Claims.Application.Queries.GetClaimStatistics;

/// <summary>
/// Query to get claim statistics for the dashboard.
/// </summary>
public sealed record GetClaimStatisticsQuery(
    Guid TenantId
) : IQuery<ClaimStatisticsDto>;
