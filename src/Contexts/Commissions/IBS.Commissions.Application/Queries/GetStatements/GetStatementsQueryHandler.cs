using IBS.BuildingBlocks.Application;
using IBS.BuildingBlocks.Application.Queries;
using IBS.Commissions.Application.DTOs;
using IBS.Commissions.Domain.Queries;

namespace IBS.Commissions.Application.Queries.GetStatements;

/// <summary>
/// Handler for the GetStatementsQuery.
/// </summary>
public sealed class GetStatementsQueryHandler(
    ICommissionStatementQueries statementQueries) : IQueryHandler<GetStatementsQuery, StatementListResult>
{
    /// <inheritdoc />
    public async Task<Result<StatementListResult>> Handle(GetStatementsQuery request, CancellationToken cancellationToken)
    {
        var filter = new StatementSearchFilter
        {
            SearchTerm = request.SearchTerm,
            CarrierId = request.CarrierId,
            Status = request.Status,
            PeriodMonth = request.PeriodMonth,
            PeriodYear = request.PeriodYear,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            SortBy = request.SortBy,
            SortDirection = request.SortDirection
        };

        var result = await statementQueries.SearchAsync(filter, cancellationToken);

        var items = result.Statements.Select(s => new CommissionStatementListItemDto(
            s.Id,
            s.CarrierId,
            s.CarrierName,
            s.StatementNumber,
            s.PeriodMonth,
            s.PeriodYear,
            s.StatementDate,
            s.Status,
            s.TotalPremium,
            s.TotalPremiumCurrency,
            s.TotalCommission,
            s.TotalCommissionCurrency,
            s.LineItemCount,
            s.ReconciledCount,
            s.DisputedCount,
            s.ReceivedAt,
            s.CreatedAt
        )).ToList();

        return new StatementListResult(
            items,
            result.TotalCount,
            result.PageNumber,
            result.PageSize,
            result.TotalPages
        );
    }
}
