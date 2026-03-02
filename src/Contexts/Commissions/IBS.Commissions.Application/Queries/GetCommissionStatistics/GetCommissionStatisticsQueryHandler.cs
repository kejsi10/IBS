using IBS.BuildingBlocks.Application;
using IBS.BuildingBlocks.Application.Queries;
using IBS.Commissions.Application.DTOs;
using IBS.Commissions.Domain.Queries;

namespace IBS.Commissions.Application.Queries.GetCommissionStatistics;

/// <summary>
/// Handler for the GetCommissionStatisticsQuery.
/// </summary>
public sealed class GetCommissionStatisticsQueryHandler(
    ICommissionStatementQueries statementQueries) : IQueryHandler<GetCommissionStatisticsQuery, CommissionStatisticsDto>
{
    /// <inheritdoc />
    public async Task<Result<CommissionStatisticsDto>> Handle(GetCommissionStatisticsQuery request, CancellationToken cancellationToken)
    {
        var stats = await statementQueries.GetStatisticsAsync(cancellationToken);

        return new CommissionStatisticsDto(
            stats.TotalStatements,
            stats.ReceivedStatements,
            stats.ReconcilingStatements,
            stats.ReconciledStatements,
            stats.PaidStatements,
            stats.DisputedStatements,
            stats.TotalCommissionAmount,
            stats.TotalPaidAmount,
            stats.TotalDisputedAmount,
            stats.StatementsByStatus,
            stats.CommissionByCarrier
        );
    }
}
