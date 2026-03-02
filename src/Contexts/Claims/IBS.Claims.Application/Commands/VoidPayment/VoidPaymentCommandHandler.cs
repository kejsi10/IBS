using IBS.BuildingBlocks.Application;
using IBS.BuildingBlocks.Application.Commands;
using IBS.BuildingBlocks.Domain;
using IBS.Claims.Domain.Repositories;

namespace IBS.Claims.Application.Commands.VoidPayment;

/// <summary>
/// Handler for the VoidPaymentCommand.
/// </summary>
public sealed class VoidPaymentCommandHandler(
    IClaimRepository claimRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<VoidPaymentCommand>
{
    /// <inheritdoc />
    public async Task<Result> Handle(VoidPaymentCommand request, CancellationToken cancellationToken)
    {
        var claim = await claimRepository.GetByIdAsync(request.ClaimId, cancellationToken);
        if (claim is null)
            return Error.NotFound("Claim", request.ClaimId);

        ConcurrencyGuard.Validate(request.ExpectedRowVersion, claim.RowVersion);

        try
        {
            claim.VoidPayment(request.PaymentId, request.Reason);
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
