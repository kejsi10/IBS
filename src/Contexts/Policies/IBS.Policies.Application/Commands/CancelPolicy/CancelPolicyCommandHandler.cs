using IBS.BuildingBlocks.Application;
using IBS.BuildingBlocks.Application.Commands;
using IBS.BuildingBlocks.Domain;
using IBS.Policies.Domain.Repositories;

namespace IBS.Policies.Application.Commands.CancelPolicy;

/// <summary>
/// Handler for the CancelPolicyCommand.
/// </summary>
public sealed class CancelPolicyCommandHandler(
    IPolicyRepository policyRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<CancelPolicyCommand>
{
    /// <inheritdoc />
    public async Task<Result> Handle(CancelPolicyCommand request, CancellationToken cancellationToken)
    {
        var policy = await policyRepository.GetByIdAsync(request.PolicyId, cancellationToken);
        if (policy is null)
        {
            return Error.NotFound("Policy", request.PolicyId);
        }

        ConcurrencyGuard.Validate(request.ExpectedRowVersion, policy.RowVersion);

        try
        {
            policy.Cancel(request.CancellationDate, request.Reason, request.CancellationType);

            await policyRepository.UpdateAsync(policy, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (BusinessRuleViolationException ex)
        {
            return Error.Validation(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return Error.Validation(ex.Message);
        }
    }
}
