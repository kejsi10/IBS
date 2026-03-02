using IBS.BuildingBlocks.Application;
using IBS.BuildingBlocks.Application.Commands;
using IBS.BuildingBlocks.Domain;
using IBS.Claims.Domain.Aggregates.Claim;
using IBS.Claims.Domain.Repositories;
using IBS.BuildingBlocks.Domain.ValueObjects;
using IBS.Claims.Domain.ValueObjects;

namespace IBS.Claims.Application.Commands.CreateClaim;

/// <summary>
/// Handler for the CreateClaimCommand.
/// </summary>
public sealed class CreateClaimCommandHandler(
    IClaimRepository claimRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<CreateClaimCommand, Guid>
{
    /// <inheritdoc />
    public async Task<Result<Guid>> Handle(CreateClaimCommand request, CancellationToken cancellationToken)
    {
        Money? estimatedLoss = null;
        if (request.EstimatedLossAmount.HasValue)
        {
            try
            {
                estimatedLoss = Money.Create(request.EstimatedLossAmount.Value, request.EstimatedLossCurrency ?? "USD");
            }
            catch (ArgumentException ex)
            {
                return Error.Validation(ex.Message);
            }
        }

        Claim claim;
        try
        {
            claim = Claim.Create(
                request.TenantId,
                request.PolicyId,
                request.ClientId,
                request.LossDate,
                request.ReportedDate,
                request.LossType,
                request.LossDescription,
                request.UserId,
                estimatedLoss);
        }
        catch (BusinessRuleViolationException ex)
        {
            return Error.Validation(ex.Message);
        }

        await claimRepository.AddAsync(claim, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return claim.Id;
    }
}
