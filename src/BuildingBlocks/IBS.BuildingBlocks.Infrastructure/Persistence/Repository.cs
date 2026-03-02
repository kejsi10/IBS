using IBS.BuildingBlocks.Domain;
using Microsoft.EntityFrameworkCore;

namespace IBS.BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// Generic repository implementation using Entity Framework Core.
/// Follows DDD patterns - only provides GetById (using FindAsync) and Add.
/// Changes to the aggregate are tracked automatically by EF Core.
/// </summary>
/// <typeparam name="TAggregate">The type of the aggregate root.</typeparam>
/// <typeparam name="TId">The type of the aggregate identifier.</typeparam>
/// <typeparam name="TContext">The type of the database context.</typeparam>
public abstract class Repository<TAggregate, TId, TContext> : IRepository<TAggregate, TId>
    where TAggregate : AggregateRoot<TId>
    where TId : notnull
    where TContext : DbContext
{
    /// <summary>
    /// Gets the database context.
    /// </summary>
    protected TContext Context { get; }

    /// <summary>
    /// Gets the DbSet for the aggregate.
    /// </summary>
    protected DbSet<TAggregate> DbSet { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Repository{TAggregate, TId, TContext}"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    protected Repository(TContext context)
    {
        Context = context;
        DbSet = context.Set<TAggregate>();
    }

    /// <inheritdoc />
    /// <remarks>
    /// Uses FindAsync which first checks if the entity is already tracked in the DbContext
    /// before querying the database. This is more efficient when the same aggregate
    /// is accessed multiple times within a unit of work.
    /// </remarks>
    public virtual async Task<TAggregate?> GetByIdAsync(TId id, CancellationToken cancellationToken = default)
    {
        return await DbSet.FindAsync([id], cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task AddAsync(TAggregate aggregate, CancellationToken cancellationToken = default)
    {
        await DbSet.AddAsync(aggregate, cancellationToken);
    }
}

/// <summary>
/// Generic repository implementation for aggregates with Guid identifiers.
/// </summary>
/// <typeparam name="TAggregate">The type of the aggregate root.</typeparam>
/// <typeparam name="TContext">The type of the database context.</typeparam>
public abstract class Repository<TAggregate, TContext> : Repository<TAggregate, Guid, TContext>, IRepository<TAggregate>
    where TAggregate : AggregateRoot
    where TContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Repository{TAggregate, TContext}"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    protected Repository(TContext context) : base(context)
    {
    }
}
