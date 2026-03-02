using IBS.BuildingBlocks.Application.Commands;
using IBS.Carriers.Domain.ValueObjects;

namespace IBS.Carriers.Application.Commands.AddProduct;

/// <summary>
/// Command to add a product to a carrier.
/// </summary>
/// <param name="CarrierId">The carrier identifier.</param>
/// <param name="Name">The product name.</param>
/// <param name="Code">The product code.</param>
/// <param name="LineOfBusiness">The line of business.</param>
/// <param name="Description">The product description (optional).</param>
/// <param name="MinimumPremium">The minimum premium (optional).</param>
/// <param name="EffectiveDate">The effective date (optional).</param>
/// <param name="ExpirationDate">The expiration date (optional).</param>
public sealed record AddProductCommand(
    Guid CarrierId,
    string Name,
    string Code,
    LineOfBusiness LineOfBusiness,
    string? Description = null,
    decimal? MinimumPremium = null,
    DateOnly? EffectiveDate = null,
    DateOnly? ExpirationDate = null,
    string? ExpectedRowVersion = null) : ICommand<Guid>;
