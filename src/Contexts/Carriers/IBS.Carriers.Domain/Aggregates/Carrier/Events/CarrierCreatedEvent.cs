using IBS.BuildingBlocks.Domain;

namespace IBS.Carriers.Domain.Aggregates.Carrier.Events;

/// <summary>
/// Domain event raised when a new carrier is created.
/// </summary>
/// <param name="CarrierId">The carrier identifier.</param>
/// <param name="Name">The carrier name.</param>
/// <param name="Code">The carrier code.</param>
public sealed record CarrierCreatedEvent(
    Guid CarrierId,
    string Name,
    string Code) : DomainEvent;
