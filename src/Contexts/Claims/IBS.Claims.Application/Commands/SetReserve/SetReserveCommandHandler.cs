using IBS.BuildingBlocks.Application;
using IBS.BuildingBlocks.Application.Commands;
using IBS.BuildingBlocks.Domain;
using IBS.Claims.Domain.Repositories;
using IBS.BuildingBlocks.Domain.ValueObjects;
using IBS.Claims.Domain.ValueObjects;

namespace IBS.Claims.Application.Commands.SetReserve;

/// <summary>
/// Handler for the SetReserveCommand.
/// </summary>
public sealed class SetReserveCommandHandler(
    IClaimRepository claimRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<SetReserveCommand>
{
    /// <inheritdoc />
    public async Task<Result> Handle(SetReserveCommand request, CancellationToken cancellationToken)
    {
        var claim = await claimRepository.GetByIdAsync(request.ClaimId, cancellationToken);
        if (claim is null)
            return Error.NotFound("Claim", request.ClaimId);

        ConcurrencyGuard.Validate(request.ExpectedRowVersion, claim.RowVersion);

        try
        {
            var amount = Money.Create(request.Amount, request.Currency);
            claim.SetReserve(request.ReserveType, amount, request.UserId.ToString(), request.Notes);
        }
        catch (ArgumentException ex)
        {
            return Error.Validation(ex.Message);
        }

        await claimRepository.UpdateAsync(claim, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
