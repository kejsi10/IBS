using IBS.BuildingBlocks.Application.Commands;

namespace IBS.Tenants.Application.Commands.ActivateTenant;

/// <summary>
/// Command to activate a suspended tenant.
/// </summary>
/// <param name="TenantId">The tenant identifier.</param>
public sealed record ActivateTenantCommand(Guid TenantId) : ICommand;
