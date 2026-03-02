using IBS.BuildingBlocks.Application;
using IBS.BuildingBlocks.Application.Commands;
using IBS.BuildingBlocks.Domain;
using IBS.Policies.Domain.Repositories;

namespace IBS.Policies.Application.Commands.SubmitQuote;

/// <summary>
/// Handler for the SubmitQuoteCommand.
/// </summary>
public sealed class SubmitQuoteCommandHandler(
    IQuoteRepository quoteRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<SubmitQuoteCommand>
{
    /// <inheritdoc />
    public async Task<Result> Handle(SubmitQuoteCommand request, CancellationToken cancellationToken)
    {
        var quote = await quoteRepository.GetByIdAsync(request.QuoteId, cancellationToken);
        if (quote is null)
        {
            return Error.NotFound("Quote", request.QuoteId);
        }

        ConcurrencyGuard.Validate(request.ExpectedRowVersion, quote.RowVersion);

        try
        {
            quote.Submit();
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (BusinessRuleViolationException ex)
        {
            return Error.Validation(ex.Message);
        }
    }
}
