using IBS.BuildingBlocks.Application;
using IBS.BuildingBlocks.Application.Commands;
using IBS.BuildingBlocks.Domain;
using IBS.Policies.Domain.Repositories;

namespace IBS.Policies.Application.Commands.RemoveCarrierFromQuote;

/// <summary>
/// Handler for the RemoveCarrierFromQuoteCommand.
/// </summary>
public sealed class RemoveCarrierFromQuoteCommandHandler(
    IQuoteRepository quoteRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<RemoveCarrierFromQuoteCommand>
{
    /// <inheritdoc />
    public async Task<Result> Handle(RemoveCarrierFromQuoteCommand request, CancellationToken cancellationToken)
    {
        var quote = await quoteRepository.GetByIdAsync(request.QuoteId, cancellationToken);
        if (quote is null)
        {
            return Error.NotFound("Quote", request.QuoteId);
        }

        ConcurrencyGuard.Validate(request.ExpectedRowVersion, quote.RowVersion);

        try
        {
            quote.RemoveCarrier(request.QuoteCarrierId);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (BusinessRuleViolationException ex)
        {
            return Error.Validation(ex.Message);
        }
    }
}
