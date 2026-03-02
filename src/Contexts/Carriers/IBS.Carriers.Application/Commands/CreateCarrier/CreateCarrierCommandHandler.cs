using IBS.BuildingBlocks.Application;
using IBS.BuildingBlocks.Application.Commands;
using IBS.BuildingBlocks.Domain;
using IBS.Carriers.Domain.Aggregates.Carrier;
using IBS.Carriers.Domain.Queries;
using IBS.Carriers.Domain.Repositories;
using IBS.Carriers.Domain.ValueObjects;

namespace IBS.Carriers.Application.Commands.CreateCarrier;

/// <summary>
/// Handler for the CreateCarrierCommand.
/// </summary>
public sealed class CreateCarrierCommandHandler(
    ICarrierRepository carrierRepository,
    ICarrierQueries carrierQueries,
    IUnitOfWork unitOfWork) : ICommandHandler<CreateCarrierCommand, Guid>
{
    /// <inheritdoc />
    public async Task<Result<Guid>> Handle(CreateCarrierCommand request, CancellationToken cancellationToken)
    {
        // Check if carrier code already exists
        if (await carrierQueries.ExistsByCodeAsync(request.Code, cancellationToken: cancellationToken))
        {
            return Error.Conflict($"A carrier with code '{request.Code}' already exists.");
        }

        // Create the carrier code value object
        var code = CarrierCode.Create(request.Code);

        // Create the carrier
        var carrier = Carrier.Create(request.Name, code, request.LegalName);

        // Set optional properties
        if (!string.IsNullOrWhiteSpace(request.AmBestRating))
        {
            var rating = AmBestRating.Create(request.AmBestRating);
            carrier.SetAmBestRating(rating);
        }

        if (!string.IsNullOrWhiteSpace(request.NaicCode))
        {
            carrier.SetNaicCode(request.NaicCode);
        }

        if (!string.IsNullOrWhiteSpace(request.WebsiteUrl))
        {
            carrier.SetWebsiteUrl(request.WebsiteUrl);
        }

        if (!string.IsNullOrWhiteSpace(request.Notes))
        {
            carrier.SetNotes(request.Notes);
        }

        // Persist the carrier
        await carrierRepository.AddAsync(carrier, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return carrier.Id;
    }
}
