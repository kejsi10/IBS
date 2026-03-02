namespace IBS.BuildingBlocks.Infrastructure.Multitenancy;

/// <summary>
/// Implementation of tenant context accessor using AsyncLocal for ambient context.
/// </summary>
public sealed class TenantContextAccessor : ITenantContextAccessor
{
    private static readonly AsyncLocal<TenantHolder> _tenantCurrent = new();

    /// <inheritdoc />
    public Guid TenantId => _tenantCurrent.Value?.TenantId ?? Guid.Empty;

    /// <inheritdoc />
    public bool HasTenant => _tenantCurrent.Value?.TenantId != null && _tenantCurrent.Value.TenantId != Guid.Empty;

    /// <inheritdoc />
    public void SetTenant(Guid tenantId)
    {
        var holder = _tenantCurrent.Value;
        if (holder != null)
        {
            holder.TenantId = Guid.Empty;
        }

        _tenantCurrent.Value = new TenantHolder { TenantId = tenantId };
    }

    /// <inheritdoc />
    public void ClearTenant()
    {
        var holder = _tenantCurrent.Value;
        if (holder != null)
        {
            holder.TenantId = Guid.Empty;
        }
    }

    private class TenantHolder
    {
        public Guid TenantId { get; set; }
    }
}
