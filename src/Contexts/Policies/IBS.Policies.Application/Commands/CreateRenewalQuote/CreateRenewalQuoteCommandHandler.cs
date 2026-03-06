using IBS.BuildingBlocks.Application;
using IBS.BuildingBlocks.Application.Commands;
using IBS.BuildingBlocks.Domain;
using IBS.Policies.Domain.Aggregates.Quote;
using IBS.Policies.Domain.Repositories;
using IBS.Policies.Domain.ValueObjects;

namespace IBS.Policies.Application.Commands.CreateRenewalQuote;

/// <summary>
/// Handler for CreateRenewalQuoteCommand.
/// Creates a new renewal quote linked to an existing policy.
/// </summary>
public sealed class CreateRenewalQuoteCommandHandler(
    IPolicyRepository policyRepository,
    IQuoteRepository quoteRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<CreateRenewalQuoteCommand, Guid>
{
    /// <inheritdoc />
    public async Task<Result<Guid>> Handle(CreateRenewalQuoteCommand request, CancellationToken cancellationToken)
    {
        var policy = await policyRepository.GetByIdAsync(request.PolicyId, cancellationToken);
        if (policy is null)
            return Error.NotFound("Policy", request.PolicyId);

        EffectivePeriod renewalPeriod;
        try
        {
            renewalPeriod = EffectivePeriod.Create(
                policy.EffectivePeriod.ExpirationDate,
                policy.EffectivePeriod.ExpirationDate.AddYears(1));
        }
        catch (ArgumentException ex)
        {
            return Error.Validation(ex.Message);
        }

        var quote = Quote.Create(
            request.TenantId,
            policy.ClientId,
            policy.LineOfBusiness,
            renewalPeriod,
            request.UserId,
            notes: $"Renewal quote for policy {policy.PolicyNumber.Value}",
            isRenewalQuote: true,
            renewalPolicyId: request.PolicyId);

        await quoteRepository.AddAsync(quote, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return quote.Id;
    }
}
