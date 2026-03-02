using IBS.Clients.Domain.Aggregates.Client;
using IBS.Clients.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace IBS.Clients.Infrastructure.Persistence;

/// <summary>
/// Repository implementation for Client aggregate.
/// Following DDD patterns - only for persisting and retrieving aggregate roots.
/// </summary>
public sealed class ClientRepository : IClientRepository
{
    private readonly DbContext _context;
    private readonly DbSet<Client> _clients;

    /// <summary>
    /// Initializes a new instance of the ClientRepository class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public ClientRepository(DbContext context)
    {
        _context = context;
        _clients = context.Set<Client>();
    }

    /// <inheritdoc />
    /// <remarks>
    /// Uses FindAsync which checks tracked entities first before hitting the database.
    /// </remarks>
    public async Task<Client?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _clients.FindAsync([id], cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Client?> GetByIdWithChildrenAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _clients
            .Include(c => c.Contacts)
            .Include(c => c.Addresses)
            .Include(c => c.Communications)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddAsync(Client aggregate, CancellationToken cancellationToken = default)
    {
        await _clients.AddAsync(aggregate, cancellationToken);
    }
}
