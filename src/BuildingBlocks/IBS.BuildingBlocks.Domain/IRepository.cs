namespace IBS.BuildingBlocks.Domain;

/// <summary>
/// Repository interface for aggregate roots following DDD patterns.
/// Repositories are only for persisting and retrieving aggregate roots.
/// For queries, use separate query handlers with no-tracking.
/// </summary>
/// <typeparam name="TAggregate">The type of the aggregate root.</typeparam>
/// <typeparam name="TId">The type of the aggregate identifier.</typeparam>
public interface IRepository<TAggregate, TId>
    where TAggregate : AggregateRoot<TId>
    where TId : notnull
{
    /// <summary>
    /// Gets an aggregate by its identifier using FindAsync (checks tracked entities first).
    /// </summary>
    /// <param name="id">The aggregate identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The aggregate if found, otherwise null.</returns>
    Task<TAggregate?> GetByIdAsync(TId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new aggregate to the repository.
    /// </summary>
    /// <param name="aggregate">The aggregate to add.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task AddAsync(TAggregate aggregate, CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository interface for aggregate roots with Guid identifiers.
/// </summary>
/// <typeparam name="TAggregate">The type of the aggregate root.</typeparam>
public interface IRepository<TAggregate> : IRepository<TAggregate, Guid>
    where TAggregate : AggregateRoot
{
}
