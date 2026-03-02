using IBS.Claims.Domain.Aggregates.Claim;
using IBS.Claims.Domain.Repositories;
using IBS.BuildingBlocks.Domain.ValueObjects;
using IBS.Claims.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace IBS.Claims.Infrastructure.Persistence;

/// <summary>
/// Repository implementation for the Claim aggregate.
/// </summary>
public sealed class ClaimRepository : IClaimRepository
{
    private readonly DbContext _context;
    private readonly DbSet<Claim> _claims;

    /// <summary>
    /// Initializes a new instance of the ClaimRepository class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public ClaimRepository(DbContext context)
    {
        _context = context;
        _claims = context.Set<Claim>();
    }

    /// <inheritdoc />
    public async Task<Claim?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _claims
            .Include(c => c.Notes)
            .Include(c => c.Reserves)
            .Include(c => c.Payments)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Claim>> GetByPolicyIdAsync(Guid policyId, CancellationToken cancellationToken = default)
    {
        return await _claims
            .Include(c => c.Notes)
            .Include(c => c.Reserves)
            .Include(c => c.Payments)
            .Where(c => c.PolicyId == policyId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Claim>> GetByClientIdAsync(Guid clientId, CancellationToken cancellationToken = default)
    {
        return await _claims
            .Include(c => c.Notes)
            .Include(c => c.Reserves)
            .Include(c => c.Payments)
            .Where(c => c.ClientId == clientId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddAsync(Claim claim, CancellationToken cancellationToken = default)
    {
        await _claims.AddAsync(claim, cancellationToken);
    }

    /// <inheritdoc />
    public Task UpdateAsync(Claim claim, CancellationToken cancellationToken = default)
    {
        // Entity is already tracked by EF change tracker via GetByIdAsync.
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task<Dictionary<ClaimStatus, int>> GetClaimCountByStatusAsync(CancellationToken cancellationToken = default)
    {
        return await _claims
            .GroupBy(c => c.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Status, x => x.Count, cancellationToken);
    }
}
