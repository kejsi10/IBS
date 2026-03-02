using IBS.BuildingBlocks.Application.Queries;
using IBS.Commissions.Application.DTOs;

namespace IBS.Commissions.Application.Queries.GetCommissionStatistics;

/// <summary>
/// Query to get commission statistics for the dashboard.
/// </summary>
public sealed record GetCommissionStatisticsQuery(
    Guid TenantId
) : IQuery<CommissionStatisticsDto>;
