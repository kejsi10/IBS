using IBS.BuildingBlocks.Application;
using IBS.BuildingBlocks.Application.Commands;
using IBS.BuildingBlocks.Domain;
using IBS.Policies.Domain.Repositories;

namespace IBS.Policies.Application.Commands.RecordQuoteResponse;

/// <summary>
/// Handler for the RecordQuoteResponseCommand.
/// </summary>
public sealed class RecordQuoteResponseCommandHandler(
    IQuoteRepository quoteRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<RecordQuoteResponseCommand>
{
    /// <inheritdoc />
    public async Task<Result> Handle(RecordQuoteResponseCommand request, CancellationToken cancellationToken)
    {
        var quote = await quoteRepository.GetByIdAsync(request.QuoteId, cancellationToken);
        if (quote is null)
        {
            return Error.NotFound("Quote", request.QuoteId);
        }

        ConcurrencyGuard.Validate(request.ExpectedRowVersion, quote.RowVersion);

        try
        {
            if (request.IsQuoted)
            {
                quote.RecordQuotedResponse(
                    request.QuoteCarrierId,
                    request.PremiumAmount!.Value,
                    request.PremiumCurrency ?? "USD",
                    request.Conditions,
                    request.ProposedCoverages,
                    request.CarrierExpiresAt);
            }
            else
            {
                quote.RecordDeclinedResponse(request.QuoteCarrierId, request.DeclinationReason);
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (BusinessRuleViolationException ex)
        {
            return Error.Validation(ex.Message);
        }
    }
}
