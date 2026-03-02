using IBS.BuildingBlocks.Domain;
using IBS.Carriers.Domain.ValueObjects;

namespace IBS.Carriers.Domain.Aggregates.Carrier.Events;

/// <summary>
/// Domain event raised when a product is added to a carrier.
/// </summary>
/// <param name="CarrierId">The carrier identifier.</param>
/// <param name="ProductId">The product identifier.</param>
/// <param name="ProductName">The product name.</param>
/// <param name="ProductCode">The product code.</param>
/// <param name="LineOfBusiness">The line of business.</param>
public sealed record ProductAddedEvent(
    Guid CarrierId,
    Guid ProductId,
    string ProductName,
    string ProductCode,
    LineOfBusiness LineOfBusiness) : DomainEvent;
