using IBS.BuildingBlocks.Application;
using IBS.BuildingBlocks.Application.Commands;
using IBS.BuildingBlocks.Domain;
using IBS.Policies.Domain.Repositories;

namespace IBS.Policies.Application.Commands.IssueEndorsement;

/// <summary>
/// Handler for the IssueEndorsementCommand.
/// </summary>
public sealed class IssueEndorsementCommandHandler(
    IPolicyRepository policyRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<IssueEndorsementCommand>
{
    /// <inheritdoc />
    public async Task<Result> Handle(IssueEndorsementCommand request, CancellationToken cancellationToken)
    {
        var policy = await policyRepository.GetByIdAsync(request.PolicyId, cancellationToken);
        if (policy is null)
        {
            return Error.NotFound("Policy", request.PolicyId);
        }

        ConcurrencyGuard.Validate(request.ExpectedRowVersion, policy.RowVersion);

        try
        {
            policy.IssueEndorsement(request.EndorsementId);

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
