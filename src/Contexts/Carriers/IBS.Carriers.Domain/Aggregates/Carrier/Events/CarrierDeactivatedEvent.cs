using IBS.BuildingBlocks.Domain;

namespace IBS.Carriers.Domain.Aggregates.Carrier.Events;

/// <summary>
/// Domain event raised when a carrier is deactivated.
/// </summary>
/// <param name="CarrierId">The carrier identifier.</param>
/// <param name="Reason">The reason for deactivation.</param>
public sealed record CarrierDeactivatedEvent(
    Guid CarrierId,
    string? Reason = null) : DomainEvent;
