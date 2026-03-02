using IBS.BuildingBlocks.Application.Commands;

namespace IBS.Tenants.Application.Commands.RemoveTenantCarrier;

/// <summary>
/// Command to remove a carrier from a tenant.
/// </summary>
/// <param name="TenantId">The tenant identifier.</param>
/// <param name="CarrierId">The carrier identifier.</param>
public sealed record RemoveTenantCarrierCommand(
    Guid TenantId,
    Guid CarrierId) : ICommand;
