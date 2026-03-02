using IBS.BuildingBlocks.Domain;

namespace IBS.Tenants.Domain.Aggregates.Tenant;

/// <summary>
/// Represents the relationship between a tenant and a carrier.
/// </summary>
public sealed class TenantCarrier : Entity
{
    /// <summary>
    /// Gets the tenant identifier.
    /// </summary>
    public Guid TenantId { get; private set; }

    /// <summary>
    /// Gets the carrier identifier.
    /// </summary>
    public Guid CarrierId { get; private set; }

    /// <summary>
    /// Gets the agency code for this carrier.
    /// </summary>
    public string? AgencyCode { get; private set; }

    /// <summary>
    /// Gets the default commission rate for this carrier.
    /// </summary>
    public decimal? CommissionRate { get; private set; }

    /// <summary>
    /// Gets a value indicating whether this carrier relationship is active.
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Required by EF Core.
    /// </summary>
    private TenantCarrier() { }

    /// <summary>
    /// Creates a new tenant-carrier relationship.
    /// </summary>
    /// <param name="tenantId">The tenant identifier.</param>
    /// <param name="carrierId">The carrier identifier.</param>
    /// <param name="agencyCode">The agency code.</param>
    /// <param name="commissionRate">The commission rate.</param>
    /// <returns>A new tenant-carrier instance.</returns>
    internal static TenantCarrier Create(Guid tenantId, Guid carrierId, string? agencyCode, decimal? commissionRate)
    {
        return new TenantCarrier
        {
            TenantId = tenantId,
            CarrierId = carrierId,
            AgencyCode = agencyCode,
            CommissionRate = commissionRate,
            IsActive = true
        };
    }

    /// <summary>
    /// Updates the agency code.
    /// </summary>
    /// <param name="agencyCode">The new agency code.</param>
    public void UpdateAgencyCode(string? agencyCode)
    {
        AgencyCode = agencyCode;
        MarkAsUpdated();
    }

    /// <summary>
    /// Updates the commission rate.
    /// </summary>
    /// <param name="commissionRate">The new commission rate.</param>
    public void UpdateCommissionRate(decimal? commissionRate)
    {
        CommissionRate = commissionRate;
        MarkAsUpdated();
    }

    /// <summary>
    /// Deactivates this carrier relationship.
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        MarkAsUpdated();
    }

    /// <summary>
    /// Activates this carrier relationship.
    /// </summary>
    public void Activate()
    {
        IsActive = true;
        MarkAsUpdated();
    }
}
