using IBS.BuildingBlocks.Application;
using IBS.BuildingBlocks.Application.Commands;
using IBS.BuildingBlocks.Domain;
using IBS.Commissions.Domain.Repositories;
using IBS.BuildingBlocks.Domain.ValueObjects;
using IBS.Commissions.Domain.ValueObjects;

namespace IBS.Commissions.Application.Commands.AddLineItem;

/// <summary>
/// Handler for the AddLineItemCommand.
/// </summary>
public sealed class AddLineItemCommandHandler(
    ICommissionStatementRepository statementRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<AddLineItemCommand, Guid>
{
    /// <inheritdoc />
    public async Task<Result<Guid>> Handle(AddLineItemCommand request, CancellationToken cancellationToken)
    {
        var statement = await statementRepository.GetByIdAsync(request.StatementId, cancellationToken);
        if (statement is null)
            return Error.NotFound("Commission statement not found.");

        ConcurrencyGuard.Validate(request.ExpectedRowVersion, statement.RowVersion);

        Money grossPremium;
        Money commissionAmount;
        try
        {
            grossPremium = Money.Create(request.GrossPremium, request.GrossPremiumCurrency, allowNegative: true);
            commissionAmount = Money.Create(request.CommissionAmount, request.CommissionAmountCurrency, allowNegative: true);
        }
        catch (ArgumentException ex)
        {
            return Error.Validation(ex.Message);
        }

        try
        {
            var lineItem = statement.AddLineItem(
                request.PolicyNumber,
                request.InsuredName,
                request.LineOfBusiness,
                request.EffectiveDate,
                request.TransactionType,
                grossPremium,
                request.CommissionRate,
                commissionAmount,
                request.PolicyId);

            await statementRepository.UpdateAsync(statement, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return lineItem.Id;
        }
        catch (BusinessRuleViolationException ex)
        {
            return Error.Validation(ex.Message);
        }
    }
}
