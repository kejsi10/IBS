using IBS.Tenants.Application.Queries;
using IBS.Tenants.Domain.Aggregates.Tenant;
using Microsoft.EntityFrameworkCore;

namespace IBS.Tenants.Infrastructure.Queries;

/// <summary>
/// Read-only query implementation for Tenant data.
/// All queries use AsNoTracking for optimal performance.
/// </summary>
public sealed class TenantQueries : ITenantQueries
{
    private readonly DbContext _context;
    private readonly DbSet<Tenant> _tenants;

    /// <summary>
    /// Initializes a new instance of the TenantQueries class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public TenantQueries(DbContext context)
    {
        _context = context;
        _tenants = context.Set<Tenant>();
    }

    /// <inheritdoc />
    public async Task<TenantDetailsDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var tenant = await _tenants
            .AsNoTracking()
            .Include(t => t.Carriers)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        if (tenant is null)
            return null;

        return new TenantDetailsDto
        {
            Id = tenant.Id,
            Name = tenant.Name,
            Subdomain = tenant.Subdomain.Value,
            Status = tenant.Status.ToString(),
            SubscriptionTier = tenant.SubscriptionTier.ToString(),
            DefaultCurrency = tenant.DefaultCurrency,
            Carriers = tenant.Carriers.Select(c => new TenantCarrierDto(
                c.CarrierId,
                c.AgencyCode,
                c.CommissionRate,
                c.IsActive)).ToList(),
            CreatedAt = tenant.CreatedAt,
            UpdatedAt = tenant.UpdatedAt
        };
    }

    /// <inheritdoc />
    public async Task<PagedResult<TenantListItemDto>> SearchAsync(
        string? searchTerm,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _tenants.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim().ToLower();
            query = query.Where(t =>
                t.Name.ToLower().Contains(term) ||
                t.Subdomain.Value.ToLower().Contains(term));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(t => t.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new TenantListItemDto
            {
                Id = t.Id,
                Name = t.Name,
                Subdomain = t.Subdomain.Value,
                Status = t.Status.ToString(),
                SubscriptionTier = t.SubscriptionTier.ToString(),
                CreatedAt = t.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<TenantListItemDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    /// <inheritdoc />
    public async Task<bool> SubdomainExistsAsync(
        string subdomain,
        Guid? excludeTenantId = null,
        CancellationToken cancellationToken = default)
    {
        var normalizedSubdomain = subdomain.Trim().ToLower();

        var query = _tenants
            .AsNoTracking()
            .Where(t => t.Subdomain.Value.ToLower() == normalizedSubdomain);

        if (excludeTenantId.HasValue)
        {
            query = query.Where(t => t.Id != excludeTenantId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }
}
