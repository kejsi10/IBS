using IBS.BuildingBlocks.Application;
using IBS.BuildingBlocks.Application.Queries;
using IBS.Commissions.Application.DTOs;
using IBS.Commissions.Domain.Queries;

namespace IBS.Commissions.Application.Queries.GetStatementById;

/// <summary>
/// Handler for the GetStatementByIdQuery.
/// </summary>
public sealed class GetStatementByIdQueryHandler(
    ICommissionStatementQueries statementQueries) : IQueryHandler<GetStatementByIdQuery, CommissionStatementDto>
{
    /// <inheritdoc />
    public async Task<Result<CommissionStatementDto>> Handle(GetStatementByIdQuery request, CancellationToken cancellationToken)
    {
        var statement = await statementQueries.GetByIdAsync(request.StatementId, cancellationToken);
        if (statement is null)
            return Error.NotFound("Commission statement not found.");

        return new CommissionStatementDto(
            statement.Id,
            statement.CarrierId,
            statement.CarrierName,
            statement.StatementNumber,
            statement.PeriodMonth,
            statement.PeriodYear,
            statement.StatementDate,
            statement.Status,
            statement.TotalPremium,
            statement.TotalPremiumCurrency,
            statement.TotalCommission,
            statement.TotalCommissionCurrency,
            statement.ReceivedAt,
            statement.CreatedAt,
            statement.UpdatedAt,
            statement.LineItems.Select(li => new CommissionLineItemDto(
                li.Id, li.PolicyId, li.PolicyNumber, li.InsuredName, li.LineOfBusiness,
                li.EffectiveDate, li.TransactionType, li.GrossPremium, li.GrossPremiumCurrency,
                li.CommissionRate, li.CommissionAmount, li.CommissionAmountCurrency,
                li.IsReconciled, li.ReconciledAt, li.DisputeReason)).ToList(),
            statement.ProducerSplits.Select(ps => new ProducerSplitDto(
                ps.Id, ps.LineItemId, ps.ProducerName, ps.ProducerId,
                ps.SplitPercentage, ps.SplitAmount, ps.SplitAmountCurrency)).ToList(),
            statement.RowVersion
        );
    }
}
