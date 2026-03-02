using IBS.BuildingBlocks.Application.Commands;

namespace IBS.Tenants.Application.Commands.CancelTenant;

/// <summary>
/// Command to cancel a tenant subscription.
/// </summary>
/// <param name="TenantId">The tenant identifier.</param>
public sealed record CancelTenantCommand(Guid TenantId) : ICommand;
