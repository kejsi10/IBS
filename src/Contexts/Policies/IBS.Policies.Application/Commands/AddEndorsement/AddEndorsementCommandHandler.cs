using IBS.BuildingBlocks.Application;
using IBS.BuildingBlocks.Application.Commands;
using IBS.BuildingBlocks.Domain;
using IBS.Policies.Domain.Repositories;
using IBS.BuildingBlocks.Domain.ValueObjects;
using IBS.Policies.Domain.ValueObjects;

namespace IBS.Policies.Application.Commands.AddEndorsement;

/// <summary>
/// Handler for the AddEndorsementCommand.
/// </summary>
public sealed class AddEndorsementCommandHandler(
    IPolicyRepository policyRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<AddEndorsementCommand, Guid>
{
    /// <inheritdoc />
    public async Task<Result<Guid>> Handle(AddEndorsementCommand request, CancellationToken cancellationToken)
    {
        var policy = await policyRepository.GetByIdAsync(request.PolicyId, cancellationToken);
        if (policy is null)
        {
            return Error.NotFound("Policy", request.PolicyId);
        }

        ConcurrencyGuard.Validate(request.ExpectedRowVersion, policy.RowVersion);

        try
        {
            var premiumChange = Money.CreateWithSign(request.PremiumChange);

            var endorsement = policy.AddEndorsement(
                request.EffectiveDate,
                request.Type,
                request.Description,
                premiumChange,
                request.Notes);

            await policyRepository.UpdateAsync(policy, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return endorsement.Id;
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
