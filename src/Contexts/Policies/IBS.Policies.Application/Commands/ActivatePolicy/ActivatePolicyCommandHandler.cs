using IBS.BuildingBlocks.Application;
using IBS.BuildingBlocks.Application.Commands;
using IBS.BuildingBlocks.Domain;
using IBS.Policies.Domain.Repositories;

namespace IBS.Policies.Application.Commands.ActivatePolicy;

/// <summary>
/// Handler for the ActivatePolicyCommand.
/// </summary>
public sealed class ActivatePolicyCommandHandler(
    IPolicyRepository policyRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<ActivatePolicyCommand>
{
    /// <inheritdoc />
    public async Task<Result> Handle(ActivatePolicyCommand request, CancellationToken cancellationToken)
    {
        var policy = await policyRepository.GetByIdAsync(request.PolicyId, cancellationToken);
        if (policy is null)
        {
            return Error.NotFound("Policy", request.PolicyId);
        }

        ConcurrencyGuard.Validate(request.ExpectedRowVersion, policy.RowVersion);

        try
        {
            policy.Activate();

            await policyRepository.UpdateAsync(policy, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (BusinessRuleViolationException ex)
        {
            return Error.Validation(ex.Message);
        }
    }
}
