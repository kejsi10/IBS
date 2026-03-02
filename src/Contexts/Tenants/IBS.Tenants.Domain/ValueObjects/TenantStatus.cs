namespace IBS.Tenants.Domain.ValueObjects;

/// <summary>
/// Represents the status of a tenant.
/// </summary>
public enum TenantStatus
{
    /// <summary>
    /// The tenant is active and can operate normally.
    /// </summary>
    Active = 1,

    /// <summary>
    /// The tenant is suspended and cannot operate.
    /// </summary>
    Suspended = 2,

    /// <summary>
    /// The tenant subscription has been cancelled.
    /// </summary>
    Cancelled = 3
}
