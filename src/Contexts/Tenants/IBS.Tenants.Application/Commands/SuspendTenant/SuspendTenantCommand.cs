using IBS.BuildingBlocks.Application.Commands;

namespace IBS.Tenants.Application.Commands.SuspendTenant;

/// <summary>
/// Command to suspend a tenant.
/// </summary>
/// <param name="TenantId">The tenant identifier.</param>
public sealed record SuspendTenantCommand(Guid TenantId) : ICommand;
