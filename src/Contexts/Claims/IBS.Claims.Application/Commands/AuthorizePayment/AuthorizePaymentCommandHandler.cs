using IBS.BuildingBlocks.Application;
using IBS.BuildingBlocks.Application.Commands;
using IBS.BuildingBlocks.Domain;
using IBS.Claims.Domain.Repositories;
using IBS.BuildingBlocks.Domain.ValueObjects;
using IBS.Claims.Domain.ValueObjects;

namespace IBS.Claims.Application.Commands.AuthorizePayment;

/// <summary>
/// Handler for the AuthorizePaymentCommand.
/// </summary>
public sealed class AuthorizePaymentCommandHandler(
    IClaimRepository claimRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<AuthorizePaymentCommand, Guid>
{
    /// <inheritdoc />
    public async Task<Result<Guid>> Handle(AuthorizePaymentCommand request, CancellationToken cancellationToken)
    {
        var claim = await claimRepository.GetByIdAsync(request.ClaimId, cancellationToken);
        if (claim is null)
            return Error.NotFound("Claim", request.ClaimId);

        ConcurrencyGuard.Validate(request.ExpectedRowVersion, claim.RowVersion);

        try
        {
            var amount = Money.Create(request.Amount, request.Currency);
            var payment = claim.AuthorizePayment(
                request.PaymentType,
                amount,
                request.PayeeName,
                request.PaymentDate,
                request.UserId.ToString(),
                request.CheckNumber);

            await claimRepository.UpdateAsync(claim, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return payment.Id;
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
