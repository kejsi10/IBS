namespace IBS.BuildingBlocks.Domain;

/// <summary>
/// Interface for entities that belong to a tenant.
/// </summary>
public interface ITenantEntity
{
    /// <summary>
    /// Gets the tenant identifier.
    /// </summary>
    Guid TenantId { get; }
}

/// <summary>
/// Base class for tenant-scoped entities.
/// </summary>
public abstract class TenantEntity : Entity, ITenantEntity
{
    /// <inheritdoc />
    public Guid TenantId { get; protected set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="TenantEntity"/> class.
    /// </summary>
    protected TenantEntity()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TenantEntity"/> class with the specified identifiers.
    /// </summary>
    /// <param name="id">The entity identifier.</param>
    /// <param name="tenantId">The tenant identifier.</param>
    protected TenantEntity(Guid id, Guid tenantId) : base(id)
    {
        TenantId = tenantId;
    }

    /// <summary>
    /// Sets the tenant identifier for this entity.
    /// </summary>
    /// <param name="tenantId">The tenant identifier.</param>
    public void SetTenantId(Guid tenantId)
    {
        if (TenantId != Guid.Empty && TenantId != tenantId)
        {
            throw new InvalidOperationException("Cannot change the tenant of an entity.");
        }

        TenantId = tenantId;
    }
}

/// <summary>
/// Base class for tenant-scoped aggregate roots.
/// </summary>
public abstract class TenantAggregateRoot : AggregateRoot, ITenantEntity
{
    /// <inheritdoc />
    public Guid TenantId { get; protected set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="TenantAggregateRoot"/> class.
    /// </summary>
    protected TenantAggregateRoot()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TenantAggregateRoot"/> class with the specified identifiers.
    /// </summary>
    /// <param name="id">The aggregate identifier.</param>
    /// <param name="tenantId">The tenant identifier.</param>
    protected TenantAggregateRoot(Guid id, Guid tenantId) : base(id)
    {
        TenantId = tenantId;
    }

    /// <summary>
    /// Sets the tenant identifier for this aggregate.
    /// </summary>
    /// <param name="tenantId">The tenant identifier.</param>
    public void SetTenantId(Guid tenantId)
    {
        if (TenantId != Guid.Empty && TenantId != tenantId)
        {
            throw new InvalidOperationException("Cannot change the tenant of an aggregate.");
        }

        TenantId = tenantId;
    }
}
