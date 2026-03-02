using IBS.BuildingBlocks.Application.Commands;
using IBS.Tenants.Domain.ValueObjects;

namespace IBS.Tenants.Application.Commands.CreateTenant;

/// <summary>
/// Command to create a new tenant.
/// </summary>
/// <param name="Name">The tenant name.</param>
/// <param name="Subdomain">The subdomain for the tenant.</param>
/// <param name="SubscriptionTier">The subscription tier.</param>
public sealed record CreateTenantCommand(
    string Name,
    string Subdomain,
    SubscriptionTier SubscriptionTier) : ICommand<Guid>;
