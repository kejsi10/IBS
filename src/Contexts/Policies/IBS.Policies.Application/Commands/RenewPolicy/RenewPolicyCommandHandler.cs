using IBS.BuildingBlocks.Application;
using IBS.BuildingBlocks.Application.Commands;
using IBS.BuildingBlocks.Domain;
using IBS.Policies.Domain.Repositories;

namespace IBS.Policies.Application.Commands.RenewPolicy;

/// <summary>
/// Handler for the RenewPolicyCommand.
/// </summary>
public sealed class RenewPolicyCommandHandler(
    IPolicyRepository policyRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<RenewPolicyCommand, Guid>
{
    /// <inheritdoc />
    public async Task<Result<Guid>> Handle(RenewPolicyCommand request, CancellationToken cancellationToken)
    {
        var policy = await policyRepository.GetByIdAsync(request.PolicyId, cancellationToken);
        if (policy is null)
        {
            return Error.NotFound("Policy", request.PolicyId);
        }

        ConcurrencyGuard.Validate(request.ExpectedRowVersion, policy.RowVersion);

        try
        {
            var renewalPolicy = policy.CreateRenewal(request.UserId);

            await policyRepository.AddAsync(renewalPolicy, cancellationToken);

            // Mark the original policy as renewed
            policy.MarkAsRenewed(renewalPolicy.Id, renewalPolicy.PolicyNumber.Value);
            await policyRepository.UpdateAsync(policy, cancellationToken);

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return renewalPolicy.Id;
        }
        catch (BusinessRuleViolationException ex)
        {
            return Error.Validation(ex.Message);
        }
    }
}
