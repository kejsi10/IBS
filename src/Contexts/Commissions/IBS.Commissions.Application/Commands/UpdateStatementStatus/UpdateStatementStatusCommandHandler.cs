using IBS.BuildingBlocks.Application;
using IBS.BuildingBlocks.Application.Commands;
using IBS.BuildingBlocks.Domain;
using IBS.Commissions.Domain.Repositories;
using IBS.BuildingBlocks.Domain.ValueObjects;
using IBS.Commissions.Domain.ValueObjects;

namespace IBS.Commissions.Application.Commands.UpdateStatementStatus;

/// <summary>
/// Handler for the UpdateStatementStatusCommand.
/// </summary>
public sealed class UpdateStatementStatusCommandHandler(
    ICommissionStatementRepository statementRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<UpdateStatementStatusCommand>
{
    /// <inheritdoc />
    public async Task<Result> Handle(UpdateStatementStatusCommand request, CancellationToken cancellationToken)
    {
        var statement = await statementRepository.GetByIdAsync(request.StatementId, cancellationToken);
        if (statement is null)
            return Error.NotFound("Commission statement not found.");

        ConcurrencyGuard.Validate(request.ExpectedRowVersion, statement.RowVersion);

        try
        {
            switch (request.NewStatus)
            {
                case StatementStatus.Reconciling:
                    if (statement.Status == StatementStatus.Disputed)
                        statement.Reopen();
                    else
                        statement.StartReconciling();
                    break;
                case StatementStatus.Reconciled:
                    statement.MarkReconciled();
                    break;
                case StatementStatus.Disputed:
                    statement.MarkDisputed();
                    break;
                case StatementStatus.Paid:
                    statement.MarkPaid();
                    break;
                default:
                    return Error.Validation($"Cannot transition to status '{request.NewStatus}'.");
            }
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
