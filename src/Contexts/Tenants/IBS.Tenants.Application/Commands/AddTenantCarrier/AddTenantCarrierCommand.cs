using IBS.BuildingBlocks.Application.Commands;

namespace IBS.Tenants.Application.Commands.AddTenantCarrier;

/// <summary>
/// Command to add a carrier to a tenant.
/// </summary>
/// <param name="TenantId">The tenant identifier.</param>
/// <param name="CarrierId">The carrier identifier.</param>
/// <param name="AgencyCode">The agency code for this carrier (optional).</param>
/// <param name="CommissionRate">The commission rate for this carrier (optional).</param>
public sealed record AddTenantCarrierCommand(
    Guid TenantId,
    Guid CarrierId,
    string? AgencyCode,
    decimal? CommissionRate) : ICommand;
