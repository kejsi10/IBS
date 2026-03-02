using IBS.BuildingBlocks.Domain;
using IBS.Clients.Domain.Aggregates.Client;

namespace IBS.Clients.Domain.Repositories;

/// <summary>
/// Repository interface for Client aggregate.
/// Following DDD patterns - only for persisting and retrieving aggregate roots.
/// For queries, use separate query handlers with IClientQueries.
/// </summary>
public interface IClientRepository : IRepository<Client>
{
    /// <summary>
    /// Gets a client by identifier with all child entities loaded.
    /// Use this when you need to modify child entities (Contacts, Addresses, Communications).
    /// </summary>
    /// <param name="id">The client identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The client with child entities if found; otherwise, null.</returns>
    Task<Client?> GetByIdWithChildrenAsync(Guid id, CancellationToken cancellationToken = default);
}
