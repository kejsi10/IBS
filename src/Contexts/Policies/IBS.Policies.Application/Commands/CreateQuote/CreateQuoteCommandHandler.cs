using IBS.BuildingBlocks.Application;
using IBS.BuildingBlocks.Application.Commands;
using IBS.BuildingBlocks.Domain;
using IBS.Policies.Domain.Aggregates.Quote;
using IBS.Policies.Domain.Repositories;
using IBS.BuildingBlocks.Domain.ValueObjects;
using IBS.Policies.Domain.ValueObjects;

namespace IBS.Policies.Application.Commands.CreateQuote;

/// <summary>
/// Handler for the CreateQuoteCommand.
/// </summary>
public sealed class CreateQuoteCommandHandler(
    IQuoteRepository quoteRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<CreateQuoteCommand, Guid>
{
    /// <inheritdoc />
    public async Task<Result<Guid>> Handle(CreateQuoteCommand request, CancellationToken cancellationToken)
    {
        EffectivePeriod effectivePeriod;
        try
        {
            effectivePeriod = EffectivePeriod.Create(request.EffectiveDate, request.ExpirationDate);
        }
        catch (ArgumentException ex)
        {
            return Error.Validation(ex.Message);
        }

        var quote = Quote.Create(
            request.TenantId,
            request.ClientId,
            request.LineOfBusiness,
            effectivePeriod,
            request.UserId,
            request.Notes,
            request.QuoteExpiresAt);

        await quoteRepository.AddAsync(quote, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return quote.Id;
    }
}
