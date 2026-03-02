using IBS.BuildingBlocks.Domain;
using IBS.BuildingBlocks.Infrastructure.Multitenancy;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IBS.BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// Base database context with multi-tenancy, audit support, and domain event dispatch.
/// </summary>
public abstract class BaseDbContext : DbContext
{
    private readonly ITenantContext _tenantContext;
    private readonly IMediator? _mediator;

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseDbContext"/> class.
    /// </summary>
    /// <param name="options">The database context options.</param>
    /// <param name="tenantContext">The tenant context.</param>
    /// <param name="mediator">The mediator for domain event dispatch.</param>
    protected BaseDbContext(DbContextOptions options, ITenantContext tenantContext, IMediator? mediator = null)
        : base(options)
    {
        _tenantContext = tenantContext;
        _mediator = mediator;
    }

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }

    /// <summary>
    /// Applies global entity conventions that must run AFTER all entity types have
    /// been discovered via <c>ApplyConfigurationsFromAssembly</c>.
    /// Derived contexts must call this at the end of their <c>OnModelCreating</c>.
    /// </summary>
    protected void ApplyGlobalConventions(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            // Apply global query filter for tenant isolation
            if (typeof(ITenantEntity).IsAssignableFrom(entityType.ClrType))
            {
                var method = typeof(BaseDbContext)
                    .GetMethod(nameof(SetTenantQueryFilter),
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                    .MakeGenericMethod(entityType.ClrType);

                method.Invoke(this, [modelBuilder]);
            }

            // Configure RowVersion as database-generated for all entities (SQL Server
            // rowversion columns are auto-generated and cannot be written to).
            // Only aggregate roots get concurrency checking — child entities are
            // protected by their aggregate root's concurrency boundary.
            if (typeof(Entity).IsAssignableFrom(entityType.ClrType))
            {
                var prop = modelBuilder.Entity(entityType.ClrType)
                    .Property(nameof(Entity.RowVersion))
                    .IsRowVersion();

                if (!typeof(IAggregateRoot).IsAssignableFrom(entityType.ClrType))
                {
                    prop.IsConcurrencyToken(false);
                }
            }
        }
    }

    /// <inheritdoc />
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var domainEvents = ProcessChangeTrackerEntries();

        var result = await base.SaveChangesAsync(cancellationToken);

        // Dispatch domain events after successful save
        await DispatchDomainEventsAsync(domainEvents, cancellationToken);

        return result;
    }

    /// <inheritdoc />
    public override int SaveChanges()
    {
        throw new NotSupportedException(
            "Synchronous SaveChanges is not supported. Use SaveChangesAsync instead.");
    }

    /// <summary>
    /// Processes all change tracker entries in a single iteration, handling:
    /// 1. Fix misdetected entity states (Modified → Added for new entities)
    /// 2. Set audit properties (CreatedAt, UpdatedAt)
    /// 3. Set tenant ID on new entities
    /// 4. Collect domain events from aggregates
    /// </summary>
    private List<IDomainEvent> ProcessChangeTrackerEntries()
    {
        var domainEvents = new List<IDomainEvent>();
        var hasTenant = _tenantContext.HasTenant;

        foreach (var entry in ChangeTracker.Entries())
        {
            // 1. Fix misdetected entity states: new entities with pre-assigned Guids
            // are sometimes detected as Modified. Check for empty RowVersion to identify them.
            if (entry.Entity is Entity && entry.State == EntityState.Modified)
            {
                if (entry.Property(nameof(Entity.RowVersion)).OriginalValue is byte[] rowVersion
                    && rowVersion.Length == 0)
                {
                    entry.State = EntityState.Added;
                }
            }

            // 2. Set audit properties
            if (entry.Entity is Entity && (entry.State == EntityState.Added || entry.State == EntityState.Modified))
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Property("CreatedAt").CurrentValue = DateTimeOffset.UtcNow;
                }

                entry.Property("UpdatedAt").CurrentValue = DateTimeOffset.UtcNow;
            }

            // 3. Set tenant ID on new entities
            if (hasTenant && entry.Entity is ITenantEntity tenantEntity && entry.State == EntityState.Added)
            {
                if (tenantEntity.TenantId == Guid.Empty)
                {
                    entry.Property(nameof(ITenantEntity.TenantId)).CurrentValue = _tenantContext.TenantId;
                }
            }

            // 4. Collect domain events from aggregates
            if (entry.Entity is IAggregateRoot aggregate && aggregate.DomainEvents.Count > 0)
            {
                domainEvents.AddRange(aggregate.DomainEvents);
                aggregate.ClearDomainEvents();
            }
        }

        return domainEvents;
    }

    private async Task DispatchDomainEventsAsync(
        List<IDomainEvent> domainEvents,
        CancellationToken cancellationToken)
    {
        if (_mediator is null || domainEvents.Count == 0)
            return;

        foreach (var domainEvent in domainEvents)
        {
            await _mediator.Publish(domainEvent, cancellationToken);
        }
    }

    private void SetTenantQueryFilter<TEntity>(ModelBuilder modelBuilder)
        where TEntity : class, ITenantEntity
    {
        modelBuilder.Entity<TEntity>().HasQueryFilter(e => e.TenantId == _tenantContext.TenantId);
    }
}
