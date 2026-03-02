using IBS.BuildingBlocks.Application;
using IBS.BuildingBlocks.Application.Commands;
using IBS.BuildingBlocks.Domain;
using IBS.Carriers.Domain.Repositories;

namespace IBS.Carriers.Application.Commands.DeactivateCarrier;

/// <summary>
/// Handler for the DeactivateCarrierCommand.
/// </summary>
public sealed class DeactivateCarrierCommandHandler(
    ICarrierRepository carrierRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<DeactivateCarrierCommand>
{
    /// <inheritdoc />
    public async Task<Result> Handle(DeactivateCarrierCommand request, CancellationToken cancellationToken)
    {
        // Get the carrier
        var carrier = await carrierRepository.GetByIdAsync(request.CarrierId, cancellationToken);
        if (carrier is null)
        {
            return Result.Failure(Error.NotFound("Carrier", request.CarrierId));
        }

        ConcurrencyGuard.Validate(request.ExpectedRowVersion, carrier.RowVersion);

        // Deactivate the carrier
        carrier.Deactivate(request.Reason);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
