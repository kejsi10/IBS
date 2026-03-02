using IBS.BuildingBlocks.Application;
using IBS.BuildingBlocks.Application.Commands;
using IBS.BuildingBlocks.Domain;
using IBS.Claims.Domain.Repositories;

namespace IBS.Claims.Application.Commands.IssuePayment;

/// <summary>
/// Handler for the IssuePaymentCommand.
/// </summary>
public sealed class IssuePaymentCommandHandler(
    IClaimRepository claimRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<IssuePaymentCommand>
{
    /// <inheritdoc />
    public async Task<Result> Handle(IssuePaymentCommand request, CancellationToken cancellationToken)
    {
        var claim = await claimRepository.GetByIdAsync(request.ClaimId, cancellationToken);
        if (claim is null)
            return Error.NotFound("Claim", request.ClaimId);

        ConcurrencyGuard.Validate(request.ExpectedRowVersion, claim.RowVersion);

        try
        {
            claim.IssuePayment(request.PaymentId);
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
