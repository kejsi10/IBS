using IBS.BuildingBlocks.Application;
using IBS.BuildingBlocks.Application.Commands;
using IBS.BuildingBlocks.Domain;
using IBS.Carriers.Domain.Repositories;
using IBS.Carriers.Domain.ValueObjects;

namespace IBS.Carriers.Application.Commands.UpdateCarrier;

/// <summary>
/// Handler for the UpdateCarrierCommand.
/// </summary>
public sealed class UpdateCarrierCommandHandler(
    ICarrierRepository carrierRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<UpdateCarrierCommand>
{
    /// <inheritdoc />
    public async Task<Result> Handle(UpdateCarrierCommand request, CancellationToken cancellationToken)
    {
        // Get the carrier
        var carrier = await carrierRepository.GetByIdAsync(request.CarrierId, cancellationToken);
        if (carrier is null)
        {
            return Result.Failure(Error.NotFound("Carrier", request.CarrierId));
        }

        ConcurrencyGuard.Validate(request.ExpectedRowVersion, carrier.RowVersion);

        // Update basic info
        carrier.UpdateBasicInfo(request.Name, request.LegalName);

        // Update A.M. Best rating
        if (!string.IsNullOrWhiteSpace(request.AmBestRating))
        {
            var rating = AmBestRating.Create(request.AmBestRating);
            carrier.SetAmBestRating(rating);
        }
        else
        {
            carrier.SetAmBestRating(null);
        }

        // Update NAIC code
        carrier.SetNaicCode(request.NaicCode);

        // Update website URL
        carrier.SetWebsiteUrl(request.WebsiteUrl);

        // Update API endpoint
        carrier.SetApiEndpoint(request.ApiEndpoint);

        // Update notes
        carrier.SetNotes(request.Notes);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
