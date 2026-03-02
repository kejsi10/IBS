using IBS.BuildingBlocks.Application;
using IBS.BuildingBlocks.Application.Commands;
using IBS.BuildingBlocks.Domain;
using IBS.Policies.Domain.Repositories;

namespace IBS.Policies.Application.Commands.AddCarrierToQuote;

/// <summary>
/// Handler for the AddCarrierToQuoteCommand.
/// </summary>
public sealed class AddCarrierToQuoteCommandHandler(
    IQuoteRepository quoteRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<AddCarrierToQuoteCommand, Guid>
{
    /// <inheritdoc />
    public async Task<Result<Guid>> Handle(AddCarrierToQuoteCommand request, CancellationToken cancellationToken)
    {
        var quote = await quoteRepository.GetByIdAsync(request.QuoteId, cancellationToken);
        if (quote is null)
        {
            return Error.NotFound("Quote", request.QuoteId);
        }

        ConcurrencyGuard.Validate(request.ExpectedRowVersion, quote.RowVersion);

        try
        {
            var quoteCarrier = quote.AddCarrier(request.CarrierId);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return quoteCarrier.Id;
        }
        catch (BusinessRuleViolationException ex)
        {
            return Error.Validation(ex.Message);
        }
    }
}
