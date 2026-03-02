using IBS.BuildingBlocks.Domain;
using IBS.Tenants.Domain.Events;
using IBS.Tenants.Domain.ValueObjects;

namespace IBS.Tenants.Domain.Aggregates.Tenant;

/// <summary>
/// Represents a tenant (broker agency) in the system.
/// </summary>
public sealed class Tenant : AggregateRoot
{
    private readonly List<TenantCarrier> _carriers = [];

    /// <summary>
    /// Gets the tenant name.
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the subdomain for this tenant.
    /// </summary>
    public Subdomain Subdomain { get; private set; } = null!;

    /// <summary>
    /// Gets the tenant status.
    /// </summary>
    public TenantStatus Status { get; private set; }

    /// <summary>
    /// Gets the subscription tier.
    /// </summary>
    public SubscriptionTier SubscriptionTier { get; private set; }

    /// <summary>
    /// Gets the default currency code (ISO 4217) for this tenant.
    /// </summary>
    public string DefaultCurrency { get; private set; } = "USD";

    /// <summary>
    /// Gets the tenant settings as JSON.
    /// </summary>
    public string? Settings { get; private set; }

    /// <summary>
    /// Gets the carriers associated with this tenant.
    /// </summary>
    public IReadOnlyCollection<TenantCarrier> Carriers => _carriers.AsReadOnly();

    /// <summary>
    /// Required by EF Core.
    /// </summary>
    private Tenant() { }

    /// <summary>
    /// Creates a new tenant.
    /// </summary>
    /// <param name="name">The tenant name.</param>
    /// <param name="subdomain">The subdomain.</param>
    /// <param name="subscriptionTier">The subscription tier.</param>
    /// <returns>A new tenant instance.</returns>
    public static Tenant Create(string name, Subdomain subdomain, SubscriptionTier subscriptionTier)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Tenant name cannot be empty.", nameof(name));

        var tenant = new Tenant
        {
            Name = name.Trim(),
            Subdomain = subdomain,
            Status = TenantStatus.Active,
            SubscriptionTier = subscriptionTier
        };

        tenant.RaiseDomainEvent(new TenantRegisteredEvent(tenant.Id, tenant.Name, tenant.Subdomain.Value));

        return tenant;
    }

    /// <summary>
    /// Updates the tenant name.
    /// </summary>
    /// <param name="name">The new name.</param>
    public void UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Tenant name cannot be empty.", nameof(name));

        Name = name.Trim();
        MarkAsUpdated();
    }

    /// <summary>
    /// Suspends the tenant.
    /// </summary>
    public void Suspend()
    {
        if (Status == TenantStatus.Cancelled)
            throw new BusinessRuleViolationException("Cannot suspend a cancelled tenant.");

        Status = TenantStatus.Suspended;
        MarkAsUpdated();

        RaiseDomainEvent(new TenantSuspendedEvent(Id));
    }

    /// <summary>
    /// Activates a suspended tenant.
    /// </summary>
    public void Activate()
    {
        if (Status == TenantStatus.Cancelled)
            throw new BusinessRuleViolationException("Cannot activate a cancelled tenant.");

        Status = TenantStatus.Active;
        MarkAsUpdated();

        RaiseDomainEvent(new TenantActivatedEvent(Id));
    }

    /// <summary>
    /// Cancels the tenant subscription.
    /// </summary>
    public void Cancel()
    {
        Status = TenantStatus.Cancelled;
        MarkAsUpdated();

        RaiseDomainEvent(new TenantCancelledEvent(Id));
    }

    /// <summary>
    /// Updates the subscription tier.
    /// </summary>
    /// <param name="tier">The new subscription tier.</param>
    public void UpdateSubscriptionTier(SubscriptionTier tier)
    {
        SubscriptionTier = tier;
        MarkAsUpdated();
    }

    /// <summary>
    /// Updates the default currency for this tenant.
    /// </summary>
    /// <param name="currency">The ISO 4217 currency code.</param>
    public void UpdateDefaultCurrency(string currency)
    {
        if (string.IsNullOrWhiteSpace(currency) || currency.Trim().Length != 3)
            throw new ArgumentException("Currency must be a 3-letter ISO code.", nameof(currency));

        DefaultCurrency = currency.Trim().ToUpperInvariant();
        MarkAsUpdated();
    }

    /// <summary>
    /// Updates tenant settings.
    /// </summary>
    /// <param name="settings">The settings JSON.</param>
    public void UpdateSettings(string? settings)
    {
        Settings = settings;
        MarkAsUpdated();
    }

    /// <summary>
    /// Associates a carrier with this tenant.
    /// </summary>
    /// <param name="carrierId">The carrier identifier.</param>
    /// <param name="agencyCode">The agency code for this carrier.</param>
    /// <param name="commissionRate">The default commission rate.</param>
    public void AddCarrier(Guid carrierId, string? agencyCode = null, decimal? commissionRate = null)
    {
        if (_carriers.Any(c => c.CarrierId == carrierId))
            throw new BusinessRuleViolationException($"Carrier {carrierId} is already associated with this tenant.");

        var tenantCarrier = TenantCarrier.Create(Id, carrierId, agencyCode, commissionRate);
        _carriers.Add(tenantCarrier);
        MarkAsUpdated();
    }

    /// <summary>
    /// Removes a carrier association.
    /// </summary>
    /// <param name="carrierId">The carrier identifier.</param>
    public void RemoveCarrier(Guid carrierId)
    {
        var carrier = _carriers.FirstOrDefault(c => c.CarrierId == carrierId);
        if (carrier is null)
            throw new BusinessRuleViolationException($"Carrier {carrierId} is not associated with this tenant.");

        _carriers.Remove(carrier);
        MarkAsUpdated();
    }
}
