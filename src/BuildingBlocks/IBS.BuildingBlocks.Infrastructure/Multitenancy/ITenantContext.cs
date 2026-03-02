namespace IBS.BuildingBlocks.Infrastructure.Multitenancy;

/// <summary>
/// Interface for accessing the current tenant context.
/// </summary>
public interface ITenantContext
{
    /// <summary>
    /// Gets the current tenant identifier.
    /// </summary>
    Guid TenantId { get; }

    /// <summary>
    /// Gets a value indicating whether a tenant context is available.
    /// </summary>
    bool HasTenant { get; }
}

/// <summary>
/// Interface for setting the current tenant context.
/// </summary>
public interface ITenantContextAccessor : ITenantContext
{
    /// <summary>
    /// Sets the current tenant identifier.
    /// </summary>
    /// <param name="tenantId">The tenant identifier.</param>
    void SetTenant(Guid tenantId);

    /// <summary>
    /// Clears the current tenant context.
    /// </summary>
    void ClearTenant();
}
