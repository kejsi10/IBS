using IBS.BuildingBlocks.Application;
using IBS.BuildingBlocks.Application.Commands;
using IBS.BuildingBlocks.Domain;
using IBS.Commissions.Domain.Repositories;
using IBS.BuildingBlocks.Domain.ValueObjects;
using IBS.Commissions.Domain.ValueObjects;

namespace IBS.Commissions.Application.Commands.AddProducerSplit;

/// <summary>
/// Handler for the AddProducerSplitCommand.
/// </summary>
public sealed class AddProducerSplitCommandHandler(
    ICommissionStatementRepository statementRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<AddProducerSplitCommand, Guid>
{
    /// <inheritdoc />
    public async Task<Result<Guid>> Handle(AddProducerSplitCommand request, CancellationToken cancellationToken)
    {
        var statement = await statementRepository.GetByIdAsync(request.StatementId, cancellationToken);
        if (statement is null)
            return Error.NotFound("Commission statement not found.");

        Money splitAmount;
        try
        {
            splitAmount = Money.Create(request.SplitAmount, request.SplitAmountCurrency);
        }
        catch (ArgumentException ex)
        {
            return Error.Validation(ex.Message);
        }

        try
        {
            var split = statement.AddProducerSplit(
                request.LineItemId,
                request.ProducerName,
                request.ProducerId,
                request.SplitPercentage,
                splitAmount);

            await statementRepository.UpdateAsync(statement, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return split.Id;
        }
        catch (BusinessRuleViolationException ex)
        {
            return Error.Validation(ex.Message);
        }
    }
}
