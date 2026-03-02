using IBS.BuildingBlocks.Application;
using IBS.BuildingBlocks.Application.Queries;
using IBS.Commissions.Application.DTOs;
using IBS.Commissions.Domain.Queries;

namespace IBS.Commissions.Application.Queries.GetCommissionSummaryReport;

/// <summary>
/// Handler for the GetCommissionSummaryReportQuery.
/// </summary>
public sealed class GetCommissionSummaryReportQueryHandler(
    ICommissionStatementQueries statementQueries) : IQueryHandler<GetCommissionSummaryReportQuery, IReadOnlyList<CommissionSummaryEntryDto>>
{
    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<CommissionSummaryEntryDto>>> Handle(
        GetCommissionSummaryReportQuery request, CancellationToken cancellationToken)
    {
        var entries = await statementQueries.GetSummaryReportAsync(
            request.CarrierId, request.PeriodMonth, request.PeriodYear, cancellationToken);

        var result = entries.Select(e => new CommissionSummaryEntryDto(
            e.CarrierId,
            e.CarrierName,
            e.PeriodMonth,
            e.PeriodYear,
            e.StatementCount,
            e.TotalPremium,
            e.TotalCommission,
            e.TotalPaid,
            e.Currency
        )).ToList();

        return result;
    }
}
