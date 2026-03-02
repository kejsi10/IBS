using IBS.BuildingBlocks.Application.Commands;
using IBS.Tenants.Domain.ValueObjects;

namespace IBS.Tenants.Application.Commands.UpdateSubscriptionTier;

/// <summary>
/// Command to update a tenant's subscription tier.
/// </summary>
/// <param name="TenantId">The tenant identifier.</param>
/// <param name="SubscriptionTier">The new subscription tier.</param>
public sealed record UpdateSubscriptionTierCommand(
    Guid TenantId,
    SubscriptionTier SubscriptionTier) : ICommand;
