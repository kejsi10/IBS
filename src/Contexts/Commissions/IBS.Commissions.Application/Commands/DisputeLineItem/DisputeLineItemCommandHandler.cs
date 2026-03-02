using IBS.BuildingBlocks.Application;
using IBS.BuildingBlocks.Application.Commands;
using IBS.BuildingBlocks.Domain;
using IBS.Commissions.Domain.Repositories;

namespace IBS.Commissions.Application.Commands.DisputeLineItem;

/// <summary>
/// Handler for the DisputeLineItemCommand.
/// </summary>
public sealed class DisputeLineItemCommandHandler(
    ICommissionStatementRepository statementRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<DisputeLineItemCommand>
{
    /// <inheritdoc />
    public async Task<Result> Handle(DisputeLineItemCommand request, CancellationToken cancellationToken)
    {
        var statement = await statementRepository.GetByIdAsync(request.StatementId, cancellationToken);
        if (statement is null)
            return Error.NotFound("Commission statement not found.");

        ConcurrencyGuard.Validate(request.ExpectedRowVersion, statement.RowVersion);

        try
        {
            statement.DisputeLineItem(request.LineItemId, request.Reason);
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
