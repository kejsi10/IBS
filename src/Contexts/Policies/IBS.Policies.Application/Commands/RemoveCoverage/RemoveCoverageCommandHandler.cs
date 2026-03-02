using IBS.BuildingBlocks.Application;
using IBS.BuildingBlocks.Application.Commands;
using IBS.BuildingBlocks.Domain;
using IBS.Policies.Domain.Repositories;

namespace IBS.Policies.Application.Commands.RemoveCoverage;

/// <summary>
/// Handler for the RemoveCoverageCommand.
/// </summary>
public sealed class RemoveCoverageCommandHandler(
    IPolicyRepository policyRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<RemoveCoverageCommand>
{
    /// <inheritdoc />
    public async Task<Result> Handle(RemoveCoverageCommand request, CancellationToken cancellationToken)
    {
        var policy = await policyRepository.GetByIdAsync(request.PolicyId, cancellationToken);
        if (policy is null)
        {
            return Error.NotFound("Policy", request.PolicyId);
        }

        ConcurrencyGuard.Validate(request.ExpectedRowVersion, policy.RowVersion);

        try
        {
            policy.RemoveCoverage(request.CoverageId);

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
