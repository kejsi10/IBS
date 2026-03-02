using IBS.BuildingBlocks.Domain;
using IBS.Clients.Domain.ValueObjects;

namespace IBS.Clients.Domain.Events;

/// <summary>
/// Event raised when a new client is registered.
/// </summary>
/// <param name="ClientId">The client identifier.</param>
/// <param name="TenantId">The tenant identifier.</param>
/// <param name="ClientType">The type of client.</param>
/// <param name="DisplayName">The client's display name.</param>
public sealed record ClientRegisteredEvent(
    Guid ClientId,
    Guid TenantId,
    ClientType ClientType,
    string DisplayName) : DomainEvent;

/// <summary>
/// Event raised when a client is updated.
/// </summary>
/// <param name="ClientId">The client identifier.</param>
/// <param name="TenantId">The tenant identifier.</param>
public sealed record ClientUpdatedEvent(
    Guid ClientId,
    Guid TenantId) : DomainEvent;

/// <summary>
/// Event raised when a client is deactivated.
/// </summary>
/// <param name="ClientId">The client identifier.</param>
/// <param name="TenantId">The tenant identifier.</param>
public sealed record ClientDeactivatedEvent(
    Guid ClientId,
    Guid TenantId) : DomainEvent;

/// <summary>
/// Event raised when a contact is added to a client.
/// </summary>
/// <param name="ClientId">The client identifier.</param>
/// <param name="TenantId">The tenant identifier.</param>
/// <param name="ContactId">The contact identifier.</param>
public sealed record ContactAddedEvent(
    Guid ClientId,
    Guid TenantId,
    Guid ContactId) : DomainEvent;

/// <summary>
/// Event raised when a contact is removed from a client.
/// </summary>
/// <param name="ClientId">The client identifier.</param>
/// <param name="TenantId">The tenant identifier.</param>
/// <param name="ContactId">The contact identifier.</param>
public sealed record ContactRemovedEvent(
    Guid ClientId,
    Guid TenantId,
    Guid ContactId) : DomainEvent;

/// <summary>
/// Event raised when an address is added to a client.
/// </summary>
/// <param name="ClientId">The client identifier.</param>
/// <param name="TenantId">The tenant identifier.</param>
/// <param name="AddressId">The address identifier.</param>
public sealed record AddressAddedEvent(
    Guid ClientId,
    Guid TenantId,
    Guid AddressId) : DomainEvent;

/// <summary>
/// Event raised when an address is removed from a client.
/// </summary>
/// <param name="ClientId">The client identifier.</param>
/// <param name="TenantId">The tenant identifier.</param>
/// <param name="AddressId">The address identifier.</param>
public sealed record AddressRemovedEvent(
    Guid ClientId,
    Guid TenantId,
    Guid AddressId) : DomainEvent;

/// <summary>
/// Event raised when a communication is logged for a client.
/// </summary>
/// <param name="ClientId">The client identifier.</param>
/// <param name="TenantId">The tenant identifier.</param>
/// <param name="CommunicationId">The communication identifier.</param>
/// <param name="CommunicationType">The type of communication.</param>
public sealed record CommunicationLoggedEvent(
    Guid ClientId,
    Guid TenantId,
    Guid CommunicationId,
    CommunicationType CommunicationType) : DomainEvent;
