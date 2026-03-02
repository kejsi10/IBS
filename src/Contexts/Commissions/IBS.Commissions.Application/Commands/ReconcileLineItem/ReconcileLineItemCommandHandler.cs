using IBS.BuildingBlocks.Application;
using IBS.BuildingBlocks.Application.Commands;
using IBS.BuildingBlocks.Domain;
using IBS.Commissions.Domain.Repositories;

namespace IBS.Commissions.Application.Commands.ReconcileLineItem;

/// <summary>
/// Handler for the ReconcileLineItemCommand.
/// </summary>
public sealed class ReconcileLineItemCommandHandler(
    ICommissionStatementRepository statementRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<ReconcileLineItemCommand>
{
    /// <inheritdoc />
    public async Task<Result> Handle(ReconcileLineItemCommand request, CancellationToken cancellationToken)
    {
        var statement = await statementRepository.GetByIdAsync(request.StatementId, cancellationToken);
        if (statement is null)
            return Error.NotFound("Commission statement not found.");

        ConcurrencyGuard.Validate(request.ExpectedRowVersion, statement.RowVersion);

        try
        {
            statement.ReconcileLineItem(request.LineItemId);
        }
        catch (BusinessRuleViolationException ex)
        {
            return Error.Validation(ex.Message);
        }

        await statementRepository.UpdateAsync(statement, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
