using IBS.BuildingBlocks.Domain;
using IBS.Carriers.Domain.ValueObjects;

namespace IBS.Carriers.Domain.Aggregates.Carrier.Events;

/// <summary>
/// Domain event raised when an appetite rule is added to a carrier.
/// </summary>
/// <param name="CarrierId">The carrier identifier.</param>
/// <param name="AppetiteId">The appetite identifier.</param>
/// <param name="LineOfBusiness">The line of business.</param>
/// <param name="States">The states covered.</param>
public sealed record AppetiteAddedEvent(
    Guid CarrierId,
    Guid AppetiteId,
    LineOfBusiness LineOfBusiness,
    string States) : DomainEvent;
