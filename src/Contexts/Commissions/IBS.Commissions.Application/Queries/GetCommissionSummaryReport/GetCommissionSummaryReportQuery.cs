using IBS.BuildingBlocks.Application.Queries;
using IBS.Commissions.Application.DTOs;

namespace IBS.Commissions.Application.Queries.GetCommissionSummaryReport;

/// <summary>
/// Query to get commission summary report by carrier and period.
/// </summary>
public sealed record GetCommissionSummaryReportQuery(
    Guid TenantId,
    Guid? CarrierId = null,
    int? PeriodMonth = null,
    int? PeriodYear = null
) : IQuery<IReadOnlyList<CommissionSummaryEntryDto>>;
