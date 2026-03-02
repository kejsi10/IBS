using IBS.BuildingBlocks.Application;
using IBS.BuildingBlocks.Application.Commands;
using IBS.BuildingBlocks.Domain;
using IBS.Commissions.Domain.Aggregates.CommissionStatement;
using IBS.Commissions.Domain.Repositories;
using IBS.BuildingBlocks.Domain.ValueObjects;
using IBS.Commissions.Domain.ValueObjects;

namespace IBS.Commissions.Application.Commands.CreateStatement;

/// <summary>
/// Handler for the CreateStatementCommand.
/// </summary>
public sealed class CreateStatementCommandHandler(
    ICommissionStatementRepository statementRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<CreateStatementCommand, Guid>
{
    /// <inheritdoc />
    public async Task<Result<Guid>> Handle(CreateStatementCommand request, CancellationToken cancellationToken)
    {
        Money totalPremium;
        Money totalCommission;
        try
        {
            totalPremium = Money.Create(request.TotalPremium, request.TotalPremiumCurrency);
            totalCommission = Money.Create(request.TotalCommission, request.TotalCommissionCurrency);
        }
        catch (ArgumentException ex)
        {
            return Error.Validation(ex.Message);
        }

        CommissionStatement statement;
        try
        {
            statement = CommissionStatement.Create(
                request.TenantId,
                request.CarrierId,
                request.CarrierName,
                request.StatementNumber,
                request.PeriodMonth,
                request.PeriodYear,
                request.StatementDate,
                totalPremium,
                totalCommission);
        }
        catch (BusinessRuleViolationException ex)
        {
            return Error.Validation(ex.Message);
        }

        await statementRepository.AddAsync(statement, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return statement.Id;
    }
}
