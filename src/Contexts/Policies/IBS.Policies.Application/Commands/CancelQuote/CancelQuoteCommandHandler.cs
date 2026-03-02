using IBS.BuildingBlocks.Application;
using IBS.BuildingBlocks.Application.Commands;
using IBS.BuildingBlocks.Domain;
using IBS.Policies.Domain.Repositories;

namespace IBS.Policies.Application.Commands.CancelQuote;

/// <summary>
/// Handler for the CancelQuoteCommand.
/// </summary>
public sealed class CancelQuoteCommandHandler(
    IQuoteRepository quoteRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<CancelQuoteCommand>
{
    /// <inheritdoc />
    public async Task<Result> Handle(CancelQuoteCommand request, CancellationToken cancellationToken)
    {
        var quote = await quoteRepository.GetByIdAsync(request.QuoteId, cancellationToken);
        if (quote is null)
        {
            return Error.NotFound("Quote", request.QuoteId);
        }

        ConcurrencyGuard.Validate(request.ExpectedRowVersion, quote.RowVersion);

        try
        {
            quote.Cancel();
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (BusinessRuleViolationException ex)
        {
            return Error.Validation(ex.Message);
        }
    }
}
