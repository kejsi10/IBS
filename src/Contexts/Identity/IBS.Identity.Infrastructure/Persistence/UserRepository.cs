using IBS.Identity.Domain.Aggregates.User;
using IBS.Identity.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace IBS.Identity.Infrastructure.Persistence;

/// <summary>
/// Repository implementation for User aggregate.
/// Following DDD patterns - only for persisting and retrieving aggregate roots.
/// </summary>
public sealed class UserRepository : IUserRepository
{
    private readonly DbContext _context;
    private readonly DbSet<User> _users;

    /// <summary>
    /// Initializes a new instance of the UserRepository class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public UserRepository(DbContext context)
    {
        _context = context;
        _users = context.Set<User>();
    }

    /// <inheritdoc />
    /// <remarks>
    /// Uses FindAsync which checks tracked entities first before hitting the database.
    /// </remarks>
    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _users.FindAsync([id], cancellationToken);
    }

    /// <inheritdoc />
    public async Task<User?> GetByIdWithRolesAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<User?> GetByEmailAsync(Guid tenantId, string email, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.Trim().ToUpperInvariant();
        return await _users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.TenantId == tenantId && u.Email.NormalizedValue == normalizedEmail, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<User?> GetByRefreshTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        return await _users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.RefreshTokens.Any(rt => rt.Token == token), cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddAsync(User aggregate, CancellationToken cancellationToken = default)
    {
        await _users.AddAsync(aggregate, cancellationToken);
    }
}
