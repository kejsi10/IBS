using IBS.BuildingBlocks.Application;
using IBS.BuildingBlocks.Application.Commands;
using IBS.BuildingBlocks.Domain;
using IBS.Carriers.Domain.ValueObjects;
using IBS.Policies.Domain.Aggregates.Policy;
using IBS.Policies.Domain.Repositories;
using IBS.BuildingBlocks.Domain.ValueObjects;
using IBS.Policies.Domain.ValueObjects;

namespace IBS.Policies.Application.Commands.AcceptQuoteCarrier;

/// <summary>
/// Handler for the AcceptQuoteCarrierCommand.
/// Creates a draft policy from the accepted carrier's quote.
/// </summary>
public sealed class AcceptQuoteCarrierCommandHandler(
    IQuoteRepository quoteRepository,
    IPolicyRepository policyRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<AcceptQuoteCarrierCommand, Guid>
{
    /// <inheritdoc />
    public async Task<Result<Guid>> Handle(AcceptQuoteCarrierCommand request, CancellationToken cancellationToken)
    {
        var quote = await quoteRepository.GetByIdAsync(request.QuoteId, cancellationToken);
        if (quote is null)
        {
            return Error.NotFound("Quote", request.QuoteId);
        }

        ConcurrencyGuard.Validate(request.ExpectedRowVersion, quote.RowVersion);

        var carrier = quote.GetCarrier(request.QuoteCarrierId);
        if (carrier is null)
        {
            return Error.NotFound("QuoteCarrier", request.QuoteCarrierId);
        }

        try
        {
            // Create a draft policy from the quote
            var policy = Policy.Create(
                quote.TenantId,
                quote.ClientId,
                carrier.CarrierId,
                quote.LineOfBusiness,
                quote.LineOfBusiness.GetDisplayName(),
                quote.EffectivePeriod,
                request.UserId,
                quoteId: quote.Id);

            await policyRepository.AddAsync(policy, cancellationToken);

            // Accept the quote, linking to the new policy
            quote.Accept(request.QuoteCarrierId, policy.Id);

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return policy.Id;
        }
        catch (BusinessRuleViolationException ex)
        {
            return Error.Validation(ex.Message);
        }
    }
}
