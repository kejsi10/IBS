using IBS.BuildingBlocks.Application;
using IBS.BuildingBlocks.Application.Queries;
using IBS.Commissions.Application.DTOs;
using IBS.Commissions.Domain.Queries;

namespace IBS.Commissions.Application.Queries.GetProducerReport;

/// <summary>
/// Handler for the GetProducerReportQuery.
/// </summary>
public sealed class GetProducerReportQueryHandler(
    ICommissionStatementQueries statementQueries) : IQueryHandler<GetProducerReportQuery, IReadOnlyList<ProducerReportEntryDto>>
{
    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<ProducerReportEntryDto>>> Handle(
        GetProducerReportQuery request, CancellationToken cancellationToken)
    {
        var entries = await statementQueries.GetProducerReportAsync(
            request.ProducerId, request.PeriodMonth, request.PeriodYear, cancellationToken);

        var result = entries.Select(e => new ProducerReportEntryDto(
            e.ProducerId,
            e.ProducerName,
            e.PeriodMonth,
            e.PeriodYear,
            e.LineItemCount,
            e.TotalSplitAmount,
            e.AverageSplitPercentage,
            e.Currency
        )).ToList();

        return result;
    }
}
