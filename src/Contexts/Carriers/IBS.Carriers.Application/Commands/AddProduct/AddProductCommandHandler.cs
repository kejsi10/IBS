using IBS.BuildingBlocks.Application;
using IBS.BuildingBlocks.Application.Commands;
using IBS.BuildingBlocks.Domain;
using IBS.Carriers.Domain.Repositories;

namespace IBS.Carriers.Application.Commands.AddProduct;

/// <summary>
/// Handler for the AddProductCommand.
/// </summary>
public sealed class AddProductCommandHandler(
    ICarrierRepository carrierRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<AddProductCommand, Guid>
{
    /// <inheritdoc />
    public async Task<Result<Guid>> Handle(AddProductCommand request, CancellationToken cancellationToken)
    {
        // Get the carrier
        var carrier = await carrierRepository.GetByIdAsync(request.CarrierId, cancellationToken);
        if (carrier is null)
        {
            return Error.NotFound("Carrier", request.CarrierId);
        }

        ConcurrencyGuard.Validate(request.ExpectedRowVersion, carrier.RowVersion);

        // Check if product code already exists for this carrier
        var existingProduct = carrier.GetProductByCode(request.Code);
        if (existingProduct is not null)
        {
            return Error.Conflict($"A product with code '{request.Code}' already exists for this carrier.");
        }

        // Add the product
        var product = carrier.AddProduct(request.Name, request.Code, request.LineOfBusiness, request.Description);

        // Set optional properties
        if (request.MinimumPremium.HasValue)
        {
            product.SetMinimumPremium(request.MinimumPremium);
        }

        if (request.EffectiveDate.HasValue || request.ExpirationDate.HasValue)
        {
            product.SetEffectivePeriod(request.EffectiveDate, request.ExpirationDate);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return product.Id;
    }
}
