using IBS.BuildingBlocks.Domain;

namespace IBS.Tenants.Domain.Events;

/// <summary>
/// Event raised when a new tenant is registered.
/// </summary>
/// <param name="TenantId">The tenant identifier.</param>
/// <param name="Name">The tenant name.</param>
/// <param name="Subdomain">The tenant subdomain.</param>
public sealed record TenantRegisteredEvent(Guid TenantId, string Name, string Subdomain) : DomainEvent;

/// <summary>
/// Event raised when a tenant is suspended.
/// </summary>
/// <param name="TenantId">The tenant identifier.</param>
public sealed record TenantSuspendedEvent(Guid TenantId) : DomainEvent;

/// <summary>
/// Event raised when a tenant is activated.
/// </summary>
/// <param name="TenantId">The tenant identifier.</param>
public sealed record TenantActivatedEvent(Guid TenantId) : DomainEvent;

/// <summary>
/// Event raised when a tenant subscription is cancelled.
/// </summary>
/// <param name="TenantId">The tenant identifier.</param>
public sealed record TenantCancelledEvent(Guid TenantId) : DomainEvent;
