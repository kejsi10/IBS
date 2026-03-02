using IBS.BuildingBlocks.Application;
using IBS.BuildingBlocks.Application.Commands;
using IBS.BuildingBlocks.Domain;
using IBS.Policies.Domain.Repositories;
using IBS.BuildingBlocks.Domain.ValueObjects;
using IBS.Policies.Domain.ValueObjects;

namespace IBS.Policies.Application.Commands.AddCoverage;

/// <summary>
/// Handler for the AddCoverageCommand.
/// </summary>
public sealed class AddCoverageCommandHandler(
    IPolicyRepository policyRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<AddCoverageCommand, Guid>
{
    /// <inheritdoc />
    public async Task<Result<Guid>> Handle(AddCoverageCommand request, CancellationToken cancellationToken)
    {
        var policy = await policyRepository.GetByIdAsync(request.PolicyId, cancellationToken);
        if (policy is null)
        {
            return Error.NotFound("Policy", request.PolicyId);
        }

        ConcurrencyGuard.Validate(request.ExpectedRowVersion, policy.RowVersion);

        try
        {
            var premium = Money.Create(request.PremiumAmount);
            var limit = request.LimitAmount.HasValue ? Money.Create(request.LimitAmount.Value) : null;
            var deductible = request.DeductibleAmount.HasValue ? Money.Create(request.DeductibleAmount.Value) : null;

            var coverage = policy.AddCoverage(
                request.Code,
                request.Name,
                premium,
                request.Description,
                limit,
                deductible,
                request.IsOptional);

            await policyRepository.UpdateAsync(policy, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return coverage.Id;
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
