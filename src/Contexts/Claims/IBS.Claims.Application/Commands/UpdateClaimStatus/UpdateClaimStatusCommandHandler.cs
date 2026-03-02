using IBS.BuildingBlocks.Application;
using IBS.BuildingBlocks.Application.Commands;
using IBS.BuildingBlocks.Domain;
using IBS.Claims.Domain.Repositories;
using IBS.BuildingBlocks.Domain.ValueObjects;
using IBS.Claims.Domain.ValueObjects;

namespace IBS.Claims.Application.Commands.UpdateClaimStatus;

/// <summary>
/// Handler for the UpdateClaimStatusCommand.
/// </summary>
public sealed class UpdateClaimStatusCommandHandler(
    IClaimRepository claimRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<UpdateClaimStatusCommand>
{
    /// <inheritdoc />
    public async Task<Result> Handle(UpdateClaimStatusCommand request, CancellationToken cancellationToken)
    {
        var claim = await claimRepository.GetByIdAsync(request.ClaimId, cancellationToken);
        if (claim is null)
            return Error.NotFound("Claim", request.ClaimId);

        ConcurrencyGuard.Validate(request.ExpectedRowVersion, claim.RowVersion);

        try
        {
            switch (request.NewStatus)
            {
                case ClaimStatus.Acknowledged:
                    claim.Acknowledge();
                    break;

                case ClaimStatus.Assigned:
                    if (string.IsNullOrWhiteSpace(request.AdjusterId))
                        return Error.Validation("Adjuster ID is required for assignment.");
                    claim.AssignAdjuster(request.AdjusterId);
                    break;

                case ClaimStatus.UnderInvestigation:
                    if (claim.Status == ClaimStatus.Closed)
                        claim.Reopen();
                    else
                        claim.StartInvestigation();
                    break;

                case ClaimStatus.Evaluation:
                    claim.Evaluate();
                    break;

                case ClaimStatus.Approved:
                    if (!request.ClaimAmount.HasValue)
                        return Error.Validation("Claim amount is required for approval.");
                    claim.Approve(Money.Create(request.ClaimAmount.Value, request.ClaimAmountCurrency ?? "USD"));
                    break;

                case ClaimStatus.Denied:
                    if (string.IsNullOrWhiteSpace(request.DenialReason))
                        return Error.Validation("Denial reason is required.");
                    claim.Deny(request.DenialReason);
                    break;

                case ClaimStatus.Settlement:
                    claim.MoveToSettlement();
                    break;

                case ClaimStatus.Closed:
                    if (string.IsNullOrWhiteSpace(request.ClosureReason))
                        return Error.Validation("Closure reason is required.");
                    claim.Close(request.ClosureReason);
                    break;

                default:
                    return Error.Validation($"Invalid status transition to '{request.NewStatus}'.");
            }
        }
        catch (BusinessRuleViolationException ex)
        {
            return Error.Validation(ex.Message);
        }

        await claimRepository.UpdateAsync(claim, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
