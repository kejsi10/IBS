using IBS.Carriers.Domain.ValueObjects;
using IBS.Policies.Domain.Aggregates.Policy;
using IBS.Policies.Domain.Repositories;
using IBS.BuildingBlocks.Domain.ValueObjects;
using IBS.Policies.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace IBS.Policies.Infrastructure.Persistence;

/// <summary>
/// Repository implementation for Policy aggregate.
/// </summary>
public sealed class PolicyRepository : IPolicyRepository
{
    private readonly DbContext _context;
    private readonly DbSet<Policy> _policies;

    /// <summary>
    /// Initializes a new instance of the PolicyRepository class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public PolicyRepository(DbContext context)
    {
        _context = context;
        _policies = context.Set<Policy>();
    }

    /// <inheritdoc />
    public async Task<Policy?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _policies
            .Include(p => p.Coverages)
            .Include(p => p.Endorsements)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Policy?> GetByPolicyNumberAsync(string policyNumber, CancellationToken cancellationToken = default)
    {
        return await _policies
            .Include(p => p.Coverages)
            .Include(p => p.Endorsements)
            .FirstOrDefaultAsync(p => p.PolicyNumber.Value == policyNumber, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Policy>> GetByClientIdAsync(Guid clientId, CancellationToken cancellationToken = default)
    {
        return await _policies
            .Include(p => p.Coverages)
            .Where(p => p.ClientId == clientId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Policy>> GetByCarrierIdAsync(Guid carrierId, CancellationToken cancellationToken = default)
    {
        return await _policies
            .Include(p => p.Coverages)
            .Where(p => p.CarrierId == carrierId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Policy>> GetByStatusAsync(PolicyStatus status, CancellationToken cancellationToken = default)
    {
        return await _policies
            .Include(p => p.Coverages)
            .Where(p => p.Status == status)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Policy>> GetByLineOfBusinessAsync(LineOfBusiness lineOfBusiness, CancellationToken cancellationToken = default)
    {
        return await _policies
            .Include(p => p.Coverages)
            .Where(p => p.LineOfBusiness == lineOfBusiness)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Policy>> GetExpiringPoliciesAsync(DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken = default)
    {
        return await _policies
            .Include(p => p.Coverages)
            .Where(p => p.Status == PolicyStatus.Active || p.Status == PolicyStatus.PendingRenewal)
            .Where(p => p.EffectivePeriod.ExpirationDate >= startDate && p.EffectivePeriod.ExpirationDate <= endDate)
            .OrderBy(p => p.EffectivePeriod.ExpirationDate)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Policy>> GetPoliciesDueForRenewalAsync(int daysUntilExpiration, CancellationToken cancellationToken = default)
    {
        var targetDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(daysUntilExpiration));
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        return await _policies
            .Include(p => p.Coverages)
            .Where(p => p.Status == PolicyStatus.Active)
            .Where(p => p.EffectivePeriod.ExpirationDate >= today && p.EffectivePeriod.ExpirationDate <= targetDate)
            .OrderBy(p => p.EffectivePeriod.ExpirationDate)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<PolicySearchResult> SearchAsync(PolicySearchFilter filter, CancellationToken cancellationToken = default)
    {
        var query = _policies
            .Include(p => p.Coverages)
            .AsQueryable();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var term = filter.SearchTerm.Trim().ToLower();
            query = query.Where(p => p.PolicyNumber.Value.ToLower().Contains(term) ||
                                     (p.CarrierPolicyNumber != null && p.CarrierPolicyNumber.ToLower().Contains(term)) ||
                                     p.PolicyType.ToLower().Contains(term));
        }

        if (filter.ClientId.HasValue)
        {
            query = query.Where(p => p.ClientId == filter.ClientId.Value);
        }

        if (filter.CarrierId.HasValue)
        {
            query = query.Where(p => p.CarrierId == filter.CarrierId.Value);
        }

        if (filter.Status.HasValue)
        {
            query = query.Where(p => p.Status == filter.Status.Value);
        }

        if (filter.LineOfBusiness.HasValue)
        {
            query = query.Where(p => p.LineOfBusiness == filter.LineOfBusiness.Value);
        }

        if (filter.EffectiveDateFrom.HasValue)
        {
            query = query.Where(p => p.EffectivePeriod.EffectiveDate >= filter.EffectiveDateFrom.Value);
        }

        if (filter.EffectiveDateTo.HasValue)
        {
            query = query.Where(p => p.EffectivePeriod.EffectiveDate <= filter.EffectiveDateTo.Value);
        }

        if (filter.ExpirationDateFrom.HasValue)
        {
            query = query.Where(p => p.EffectivePeriod.ExpirationDate >= filter.ExpirationDateFrom.Value);
        }

        if (filter.ExpirationDateTo.HasValue)
        {
            query = query.Where(p => p.EffectivePeriod.ExpirationDate <= filter.ExpirationDateTo.Value);
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply sorting
        query = filter.SortBy?.ToLower() switch
        {
            "policynumber" => filter.SortDirection?.ToLower() == "asc"
                ? query.OrderBy(p => p.PolicyNumber.Value)
                : query.OrderByDescending(p => p.PolicyNumber.Value),
            "effectivedate" => filter.SortDirection?.ToLower() == "asc"
                ? query.OrderBy(p => p.EffectivePeriod.EffectiveDate)
                : query.OrderByDescending(p => p.EffectivePeriod.EffectiveDate),
            "expirationdate" => filter.SortDirection?.ToLower() == "asc"
                ? query.OrderBy(p => p.EffectivePeriod.ExpirationDate)
                : query.OrderByDescending(p => p.EffectivePeriod.ExpirationDate),
            "status" => filter.SortDirection?.ToLower() == "asc"
                ? query.OrderBy(p => p.Status)
                : query.OrderByDescending(p => p.Status),
            "totalpremium" => filter.SortDirection?.ToLower() == "asc"
                ? query.OrderBy(p => p.TotalPremium.Amount)
                : query.OrderByDescending(p => p.TotalPremium.Amount),
            _ => filter.SortDirection?.ToLower() == "asc"
                ? query.OrderBy(p => p.CreatedAt)
                : query.OrderByDescending(p => p.CreatedAt)
        };

        // Apply pagination
        var policies = await query
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync(cancellationToken);

        return new PolicySearchResult
        {
            Policies = policies,
            TotalCount = totalCount,
            PageNumber = filter.PageNumber,
            PageSize = filter.PageSize
        };
    }

    /// <inheritdoc />
    public async Task<bool> PolicyNumberExistsAsync(string policyNumber, Guid? excludePolicyId = null, CancellationToken cancellationToken = default)
    {
        var query = _policies.Where(p => p.PolicyNumber.Value == policyNumber);

        if (excludePolicyId.HasValue)
        {
            query = query.Where(p => p.Id != excludePolicyId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddAsync(Policy policy, CancellationToken cancellationToken = default)
    {
        await _policies.AddAsync(policy, cancellationToken);
    }

    /// <inheritdoc />
    public Task UpdateAsync(Policy policy, CancellationToken cancellationToken = default)
    {
        // Entity is already tracked by EF change tracker via GetByIdAsync.
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task<Dictionary<PolicyStatus, int>> GetPolicyCountByStatusAsync(CancellationToken cancellationToken = default)
    {
        return await _policies
            .GroupBy(p => p.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Status, x => x.Count, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Dictionary<LineOfBusiness, decimal>> GetTotalPremiumByLineOfBusinessAsync(int effectiveYear, CancellationToken cancellationToken = default)
    {
        return await _policies
            .Where(p => p.EffectivePeriod.EffectiveDate.Year == effectiveYear)
            .Where(p => p.Status != PolicyStatus.Draft && p.Status != PolicyStatus.Cancelled)
            .GroupBy(p => p.LineOfBusiness)
            .Select(g => new { LineOfBusiness = g.Key, TotalPremium = g.Sum(p => p.TotalPremium.Amount) })
            .ToDictionaryAsync(x => x.LineOfBusiness, x => x.TotalPremium, cancellationToken);
    }
}
