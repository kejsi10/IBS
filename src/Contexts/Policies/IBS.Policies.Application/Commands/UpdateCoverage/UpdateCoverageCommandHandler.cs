using IBS.BuildingBlocks.Application;
using IBS.BuildingBlocks.Application.Commands;
using IBS.BuildingBlocks.Domain;
using IBS.Policies.Domain.Repositories;
using IBS.BuildingBlocks.Domain.ValueObjects;
using IBS.Policies.Domain.ValueObjects;

namespace IBS.Policies.Application.Commands.UpdateCoverage;

/// <summary>
/// Handler for the UpdateCoverageCommand.
/// </summary>
public sealed class UpdateCoverageCommandHandler(
    IPolicyRepository policyRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<UpdateCoverageCommand>
{
    /// <inheritdoc />
    public async Task<Result> Handle(UpdateCoverageCommand request, CancellationToken cancellationToken)
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
            var perOccurrenceLimit = request.PerOccurrenceLimit.HasValue ? Money.Create(request.PerOccurrenceLimit.Value) : null;
            var aggregateLimit = request.AggregateLimit.HasValue ? Money.Create(request.AggregateLimit.Value) : null;
            var deductible = request.DeductibleAmount.HasValue ? Money.Create(request.DeductibleAmount.Value) : null;

            policy.UpdateCoverage(
                request.CoverageId,
                request.Name,
                request.Description,
                limit,
                perOccurrenceLimit,
                aggregateLimit,
                deductible,
                premium);

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
