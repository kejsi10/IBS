using IBS.BuildingBlocks.Application.Commands;

namespace IBS.Tenants.Application.Commands.UpdateTenant;

/// <summary>
/// Command to update a tenant.
/// </summary>
/// <param name="TenantId">The tenant identifier.</param>
/// <param name="Name">The new tenant name.</param>
/// <param name="Settings">The tenant settings JSON (optional).</param>
public sealed record UpdateTenantCommand(
    Guid TenantId,
    string Name,
    string? Settings) : ICommand;
