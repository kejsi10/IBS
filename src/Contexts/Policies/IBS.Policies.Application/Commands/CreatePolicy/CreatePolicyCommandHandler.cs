using IBS.BuildingBlocks.Application;
using IBS.BuildingBlocks.Application.Commands;
using IBS.BuildingBlocks.Domain;
using IBS.Policies.Domain.Aggregates.Policy;
using IBS.Policies.Domain.Repositories;
using IBS.BuildingBlocks.Domain.ValueObjects;
using IBS.Policies.Domain.ValueObjects;

namespace IBS.Policies.Application.Commands.CreatePolicy;

/// <summary>
/// Handler for the CreatePolicyCommand.
/// </summary>
public sealed class CreatePolicyCommandHandler(
    IPolicyRepository policyRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<CreatePolicyCommand, Guid>
{
    /// <inheritdoc />
    public async Task<Result<Guid>> Handle(CreatePolicyCommand request, CancellationToken cancellationToken)
    {
        // Validate effective period
        EffectivePeriod effectivePeriod;
        try
        {
            effectivePeriod = EffectivePeriod.Create(request.EffectiveDate, request.ExpirationDate);
        }
        catch (ArgumentException ex)
        {
            return Error.Validation(ex.Message);
        }

        // Create the policy
        var policy = Policy.Create(
            request.TenantId,
            request.ClientId,
            request.CarrierId,
            request.LineOfBusiness,
            request.PolicyType,
            effectivePeriod,
            request.UserId,
            billingType: request.BillingType,
            paymentPlan: request.PaymentPlan,
            quoteId: request.QuoteId);

        if (!string.IsNullOrWhiteSpace(request.Notes))
        {
            policy.SetNotes(request.Notes);
        }

        await policyRepository.AddAsync(policy, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return policy.Id;
    }
}
