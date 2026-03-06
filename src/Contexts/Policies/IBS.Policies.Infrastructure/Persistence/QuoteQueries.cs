using IBS.Carriers.Domain.ValueObjects;
using IBS.Policies.Domain.Aggregates.Quote;
using IBS.Policies.Domain.Queries;
using IBS.BuildingBlocks.Domain.ValueObjects;
using IBS.Policies.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace IBS.Policies.Infrastructure.Persistence;

/// <summary>
/// Read-side query implementation for quotes.
/// </summary>
public sealed class QuoteQueries : IQuoteQueries
{
    private readonly DbContext _context;

    /// <summary>
    /// Initializes a new instance of the QuoteQueries class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public QuoteQueries(DbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<QuoteDetailReadModel?> GetByIdAsync(Guid tenantId, Guid quoteId, CancellationToken cancellationToken = default)
    {
        var quote = await _context.Set<Quote>()
            .AsNoTracking()
            .Include(q => q.Carriers)
            .Where(q => q.TenantId == tenantId && q.Id == quoteId)
            .FirstOrDefaultAsync(cancellationToken);

        if (quote is null)
            return null;

        return new QuoteDetailReadModel(
            quote.Id,
            quote.ClientId,
            null, // ClientName resolved at API layer or via join
            quote.LineOfBusiness.GetDisplayName(),
            quote.EffectivePeriod.EffectiveDate,
            quote.EffectivePeriod.ExpirationDate,
            quote.Status.GetDisplayName(),
            quote.ExpiresAt,
            quote.AcceptedCarrierId,
            quote.PolicyId,
            quote.Notes,
            quote.CreatedBy,
            quote.CreatedAt,
            quote.UpdatedAt,
            quote.Carriers.Select(c => new QuoteCarrierReadModel(
                c.Id,
                c.CarrierId,
                null, // CarrierName resolved at API layer or via join
                c.Status.GetDisplayName(),
                c.PremiumAmount,
                c.PremiumCurrency,
                c.DeclinationReason,
                c.Conditions,
                c.ProposedCoverages,
                c.RespondedAt,
                c.ExpiresAt
            )).ToList(),
            Convert.ToBase64String(quote.RowVersion));
    }

    /// <inheritdoc />
    public async Task<QuoteSearchResult> SearchAsync(Guid tenantId, QuoteSearchFilter filter, CancellationToken cancellationToken = default)
    {
        var query = _context.Set<Quote>()
            .AsNoTracking()
            .Include(q => q.Carriers)
            .Where(q => q.TenantId == tenantId)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var term = filter.SearchTerm.Trim().ToLower();
            query = query.Where(q => (q.Notes != null && q.Notes.ToLower().Contains(term)));
        }

        if (filter.ClientId.HasValue)
        {
            query = query.Where(q => q.ClientId == filter.ClientId.Value);
        }

        if (filter.Status.HasValue)
        {
            query = query.Where(q => q.Status == filter.Status.Value);
        }

        if (filter.LineOfBusiness.HasValue)
        {
            query = query.Where(q => q.LineOfBusiness == filter.LineOfBusiness.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        query = filter.SortBy?.ToLower() switch
        {
            "effectivedate" => filter.SortDirection?.ToLower() == "asc"
                ? query.OrderBy(q => q.EffectivePeriod.EffectiveDate)
                : query.OrderByDescending(q => q.EffectivePeriod.EffectiveDate),
            "expiresat" => filter.SortDirection?.ToLower() == "asc"
                ? query.OrderBy(q => q.ExpiresAt)
                : query.OrderByDescending(q => q.ExpiresAt),
            "status" => filter.SortDirection?.ToLower() == "asc"
                ? query.OrderBy(q => q.Status)
                : query.OrderByDescending(q => q.Status),
            _ => filter.SortDirection?.ToLower() == "asc"
                ? query.OrderBy(q => q.CreatedAt)
                : query.OrderByDescending(q => q.CreatedAt)
        };

        var quotes = await query
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync(cancellationToken);

        var items = quotes.Select(q => new QuoteListItemReadModel(
            q.Id,
            q.ClientId,
            null,
            q.LineOfBusiness.GetDisplayName(),
            q.EffectivePeriod.EffectiveDate,
            q.EffectivePeriod.ExpirationDate,
            q.Status.GetDisplayName(),
            q.ExpiresAt,
            q.Carriers.Count,
            q.Carriers.Count(c => c.Status != QuoteCarrierStatus.Pending),
            q.Carriers.Where(c => c.Status == QuoteCarrierStatus.Quoted && c.PremiumAmount.HasValue)
                .Select(c => c.PremiumAmount!.Value)
                .DefaultIfEmpty(0)
                .Min(),
            q.CreatedAt
        )).ToList();

        // Fix: if lowest premium is 0 and no quoted carriers, set to null
        items = items.Select(i => i with
        {
            LowestPremium = i.LowestPremium == 0 ? null : i.LowestPremium
        }).ToList();

        var totalPages = (int)Math.Ceiling((double)totalCount / filter.PageSize);

        return new QuoteSearchResult(items, totalCount, filter.PageNumber, filter.PageSize, totalPages);
    }

    /// <inheritdoc />
    public async Task<QuoteSearchResult> GetByClientIdAsync(Guid tenantId, Guid clientId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var filter = new QuoteSearchFilter
        {
            ClientId = clientId,
            PageNumber = page,
            PageSize = pageSize
        };

        return await SearchAsync(tenantId, filter, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<QuoteDetailReadModel>> GetRenewalQuotesAsync(
        Guid tenantId,
        Guid renewalPolicyId,
        CancellationToken cancellationToken = default)
    {
        var quotes = await _context.Set<Quote>()
            .AsNoTracking()
            .Include(q => q.Carriers)
            .Where(q => q.TenantId == tenantId && q.IsRenewalQuote && q.RenewalPolicyId == renewalPolicyId)
            .OrderByDescending(q => q.CreatedAt)
            .ToListAsync(cancellationToken);

        return quotes.Select(quote => new QuoteDetailReadModel(
            quote.Id,
            quote.ClientId,
            null,
            quote.LineOfBusiness.GetDisplayName(),
            quote.EffectivePeriod.EffectiveDate,
            quote.EffectivePeriod.ExpirationDate,
            quote.Status.GetDisplayName(),
            quote.ExpiresAt,
            quote.AcceptedCarrierId,
            quote.PolicyId,
            quote.Notes,
            quote.CreatedBy,
            quote.CreatedAt,
            quote.UpdatedAt,
            quote.Carriers.Select(c => new QuoteCarrierReadModel(
                c.Id,
                c.CarrierId,
                null,
                c.Status.GetDisplayName(),
                c.PremiumAmount,
                c.PremiumCurrency,
                c.DeclinationReason,
                c.Conditions,
                c.ProposedCoverages,
                c.RespondedAt,
                c.ExpiresAt
            )).ToList(),
            Convert.ToBase64String(quote.RowVersion)
        )).ToList();
    }

    /// <inheritdoc />
    public async Task<QuoteSummaryStats> GetSummaryAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var quotes = _context.Set<Quote>()
            .AsNoTracking()
            .Where(q => q.TenantId == tenantId);

        var totalQuotes = await quotes.CountAsync(cancellationToken);

        var statusCounts = await quotes
            .GroupBy(q => q.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Status, x => x.Count, cancellationToken);

        var avgPremium = await _context.Set<QuoteCarrier>()
            .AsNoTracking()
            .Where(qc => qc.PremiumAmount.HasValue && qc.Status == QuoteCarrierStatus.Quoted)
            .Join(quotes, qc => qc.QuoteId, q => q.Id, (qc, q) => qc.PremiumAmount!.Value)
            .DefaultIfEmpty()
            .AverageAsync(cancellationToken);

        return new QuoteSummaryStats(
            totalQuotes,
            statusCounts.GetValueOrDefault(QuoteStatus.Draft),
            statusCounts.GetValueOrDefault(QuoteStatus.Submitted),
            statusCounts.GetValueOrDefault(QuoteStatus.Quoted),
            statusCounts.GetValueOrDefault(QuoteStatus.Accepted),
            statusCounts.GetValueOrDefault(QuoteStatus.Expired),
            statusCounts.GetValueOrDefault(QuoteStatus.Cancelled),
            avgPremium == 0 ? null : Math.Round(avgPremium, 2));
    }
}
