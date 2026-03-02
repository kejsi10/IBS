using IBS.BuildingBlocks.Domain;

namespace IBS.Identity.Domain.Events;

/// <summary>
/// Event raised when a new user is registered.
/// </summary>
/// <param name="UserId">The user identifier.</param>
/// <param name="TenantId">The tenant identifier.</param>
/// <param name="Email">The user's email address.</param>
public sealed record UserRegisteredEvent(
    Guid UserId,
    Guid TenantId,
    string Email) : DomainEvent;

/// <summary>
/// Event raised when a user account is activated.
/// </summary>
/// <param name="UserId">The user identifier.</param>
/// <param name="TenantId">The tenant identifier.</param>
public sealed record UserActivatedEvent(
    Guid UserId,
    Guid TenantId) : DomainEvent;

/// <summary>
/// Event raised when a user account is deactivated.
/// </summary>
/// <param name="UserId">The user identifier.</param>
/// <param name="TenantId">The tenant identifier.</param>
public sealed record UserDeactivatedEvent(
    Guid UserId,
    Guid TenantId) : DomainEvent;

/// <summary>
/// Event raised when a role is assigned to a user.
/// </summary>
/// <param name="UserId">The user identifier.</param>
/// <param name="TenantId">The tenant identifier.</param>
/// <param name="RoleId">The role identifier.</param>
public sealed record RoleAssignedEvent(
    Guid UserId,
    Guid TenantId,
    Guid RoleId) : DomainEvent;

/// <summary>
/// Event raised when a permission is granted to a role.
/// </summary>
/// <param name="RoleId">The role identifier.</param>
/// <param name="TenantId">The tenant identifier.</param>
/// <param name="PermissionId">The permission identifier.</param>
public sealed record PermissionGrantedEvent(
    Guid RoleId,
    Guid TenantId,
    Guid PermissionId) : DomainEvent;
