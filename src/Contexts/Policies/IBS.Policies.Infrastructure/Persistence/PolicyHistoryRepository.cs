using IBS.Policies.Domain.Aggregates.Policy;
using IBS.Policies.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace IBS.Policies.Infrastructure.Persistence;

/// <summary>
/// EF Core implementation of IPolicyHistoryRepository.
/// </summary>
public sealed class PolicyHistoryRepository(DbContext context) : IPolicyHistoryRepository
{
    /// <inheritdoc />
    public async Task AddAsync(PolicyHistory entry, CancellationToken cancellationToken = default)
    {
        await context.Set<PolicyHistory>().AddAsync(entry, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<PolicyHistoryPage> GetByPolicyIdAsync(
        Guid tenantId,
        Guid policyId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = context.Set<PolicyHistory>()
            .Where(h => h.TenantId == tenantId && h.PolicyId == policyId)
            .OrderByDescending(h => h.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PolicyHistoryPage
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = page,
            PageSize = pageSize,
        };
    }
}
